using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using AttendAI.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace AttendAI.Infrastructure.FaceRecognition;

public sealed class RealFaceRecognitionModelStore
{
    private const long MinimumDetectorBytes = 100_000;
    private const long MinimumRecognizerBytes = 1_000_000;
    private readonly IHostEnvironment _environment;
    private readonly FaceRecognitionOptions _options;

    public RealFaceRecognitionModelStore(
        IOptions<FaceRecognitionOptions> options,
        IHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public RealFaceModelReadiness CheckReadiness(bool loadNativeModels = true)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.X64)
        {
            return Failure("UnsupportedRuntime", "Real face recognition requires Windows x64.");
        }

        RealFaceModelPaths paths;
        try
        {
            paths = ResolvePaths();
        }
        catch (InvalidOperationException exception)
        {
            return Failure("UnsafeModelPath", exception.Message);
        }

        var fileCheck = ValidateModelFile(paths.DetectorModelPath, _options.RealEngine.DetectorModelSha256, MinimumDetectorBytes);
        if (!fileCheck.Succeeded)
        {
            return Failure($"Detector{fileCheck.ErrorCode}", fileCheck.Message);
        }

        fileCheck = ValidateModelFile(paths.RecognizerModelPath, _options.RealEngine.RecognizerModelSha256, MinimumRecognizerBytes);
        if (!fileCheck.Succeeded)
        {
            return Failure($"Recognizer{fileCheck.ErrorCode}", fileCheck.Message);
        }

        string openCvVersion;
        try
        {
            openCvVersion = Cv2.GetVersionString() ?? "Unknown";
        }
        catch (Exception exception) when (IsNativeFailure(exception))
        {
            return Failure("OpenCvNativeUnavailable", "OpenCV native runtime could not be initialized.");
        }

        if (!loadNativeModels)
        {
            return Success(paths, openCvVersion);
        }

        try
        {
            using var detector = FaceDetectorYN.Create(
                paths.DetectorModelPath,
                string.Empty,
                new Size(
                    Math.Max(1, _options.RealEngine.DetectorInputWidth),
                    Math.Max(1, _options.RealEngine.DetectorInputHeight)),
                (float)Math.Clamp(_options.RealEngine.DetectionScoreThreshold, 0.01, 0.99),
                (float)Math.Clamp(_options.RealEngine.DetectionNmsThreshold, 0.01, 0.99),
                Math.Max(1, _options.RealEngine.DetectionTopK),
                Backend.OPENCV,
                Target.CPU);
        }
        catch (Exception exception) when (IsNativeFailure(exception))
        {
            return Failure("DetectorLoadFailed", "YuNet detector model could not be loaded.");
        }

        try
        {
            using var session = CreateRecognizerSession(paths.RecognizerModelPath);
            var inspection = InspectRecognizerSession(session);
            if (!inspection.Succeeded)
            {
                return Failure(inspection.ErrorCode, inspection.Message, openCvVersion);
            }

            return Success(paths, openCvVersion, inspection.Metadata);
        }
        catch (Exception exception) when (exception is OnnxRuntimeException or ArgumentException or InvalidOperationException or DllNotFoundException or BadImageFormatException)
        {
            return Failure("RecognizerLoadFailed", "SFace recognizer model could not be loaded.", openCvVersion);
        }
    }

    public RealFaceModelPaths ResolvePaths()
    {
        var approvedRootPath = ResolvePath(_options.RealEngine.ApprovedModelRoot);
        var detectorPath = ResolvePath(_options.RealEngine.DetectorModelPath);
        var recognizerPath = ResolvePath(_options.RealEngine.RecognizerModelPath);

        if (!IsUnderRoot(detectorPath, approvedRootPath) || !IsUnderRoot(recognizerPath, approvedRootPath))
        {
            throw new InvalidOperationException("Configured face-recognition model paths must stay under the approved model root.");
        }

        return new RealFaceModelPaths(
            approvedRootPath,
            detectorPath,
            recognizerPath,
            SafeIdentifier(detectorPath, approvedRootPath),
            SafeIdentifier(recognizerPath, approvedRootPath));
    }

    public static bool IsGitLfsPointer(byte[] bytes)
    {
        var prefixLength = Math.Min(bytes.Length, 128);
        var prefix = Encoding.ASCII.GetString(bytes, 0, prefixLength);
        return prefix.StartsWith("version https://git-lfs.github.com/spec", StringComparison.Ordinal);
    }

    public static double CosineSimilarity(ReadOnlySpan<float> first, ReadOnlySpan<float> second)
    {
        if (first.Length != second.Length || first.Length == 0)
        {
            throw new ArgumentException("Embeddings must have the same non-empty dimension.");
        }

        double sum = 0;
        for (var index = 0; index < first.Length; index++)
        {
            sum += first[index] * second[index];
        }

        return Math.Clamp(sum, -1d, 1d);
    }

    internal static InferenceSession CreateRecognizerSession(string modelPath)
    {
        using var sessionOptions = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_BASIC,
            LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR
        };

        return new InferenceSession(modelPath, sessionOptions);
    }

    internal static (bool Succeeded, string ErrorCode, string Message, RealFaceRecognizerTensorMetadata? Metadata) InspectRecognizerSession(InferenceSession session)
    {
        if (session.InputMetadata.Count != 1 || session.OutputMetadata.Count == 0)
        {
            return (false, "RecognizerMetadataInvalid", "SFace recognizer must expose one input and at least one output.", null);
        }

        var input = session.InputMetadata.Values.First();
        if (!IsFloatTensor(input.ElementType))
        {
            return (false, "RecognizerInputInvalid", "SFace recognizer input must be a float tensor.", null);
        }

        if (input.Dimensions.Length != 4 ||
            !DimensionMatches(input.Dimensions[1], 3) ||
            !DimensionMatches(input.Dimensions[2], RealFaceRecognitionMetadata.AlignedFaceHeight) ||
            !DimensionMatches(input.Dimensions[3], RealFaceRecognitionMetadata.AlignedFaceWidth))
        {
            return (false, "RecognizerInputShapeInvalid", "SFace recognizer input must be compatible with [1, 3, 112, 112].", null);
        }

        var output = session.OutputMetadata.Values.First();
        if (!IsFloatTensor(output.ElementType))
        {
            return (false, "RecognizerOutputInvalid", "SFace recognizer output must be a float tensor.", null);
        }

        if (output.Dimensions.Length == 0 ||
            output.Dimensions[^1] != RealFaceRecognitionMetadata.SFaceEmbeddingDimension)
        {
            return (false, "RecognizerEmbeddingDimensionInvalid", "SFace recognizer output dimension is not the expected 128.", null);
        }

        var metadata = new RealFaceRecognizerTensorMetadata(
            ElementTypeName(input.ElementType),
            Shape(input.Dimensions),
            ElementTypeName(output.ElementType),
            Shape(output.Dimensions),
            output.Dimensions[^1]);
        return (true, string.Empty, string.Empty, metadata);
    }

    private static bool DimensionMatches(int actual, int expected)
        => actual == expected;

    private static bool IsFloatTensor(object elementType)
    {
        if (elementType is Type type)
        {
            return type == typeof(float);
        }

        var name = elementType.ToString();
        return name?.Contains("Float", StringComparison.OrdinalIgnoreCase) == true ||
               name?.Contains("Single", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string ElementTypeName(object elementType)
        => elementType is Type type ? type.FullName ?? type.Name : elementType.ToString() ?? "Unknown";

    private static string Shape(IReadOnlyList<int> dimensions)
        => $"[{string.Join(", ", dimensions)}]";

    private (bool Succeeded, string ErrorCode, string Message) ValidateModelFile(
        string path,
        string expectedSha256,
        long minimumBytes)
    {
        if (!File.Exists(path))
        {
            return (false, "Missing", "Configured model file does not exist.");
        }

        var bytes = File.ReadAllBytes(path);
        if (IsGitLfsPointer(bytes))
        {
            return (false, "GitLfsPointer", "Configured model file is a Git LFS pointer, not an ONNX model.");
        }

        var fileInfo = new FileInfo(path);
        if (fileInfo.Length < minimumBytes)
        {
            return (false, "TooSmall", "Configured model file is smaller than expected.");
        }

        if (string.IsNullOrWhiteSpace(expectedSha256))
        {
            return (false, "HashMissing", "Configured model SHA-256 hash is missing.");
        }

        var actualSha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return string.Equals(actualSha256, expectedSha256.Trim(), StringComparison.OrdinalIgnoreCase)
            ? (true, string.Empty, string.Empty)
            : (false, "HashMismatch", "Configured model SHA-256 hash does not match.");
    }

    private string ResolvePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("Configured face-recognition model path is empty.");
        }

        var candidate = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_environment.ContentRootPath, configuredPath);

        return Path.GetFullPath(candidate);
    }

    private static bool IsUnderRoot(string path, string root)
    {
        var normalizedRoot = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string SafeIdentifier(string path, string root)
        => Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');

    private static bool IsNativeFailure(Exception exception)
        => exception is OpenCVException or DllNotFoundException or BadImageFormatException or TypeInitializationException or InvalidOperationException;

    private static RealFaceModelReadiness Success(
        RealFaceModelPaths paths,
        string openCvVersion,
        RealFaceRecognizerTensorMetadata? recognizerMetadata = null)
        => new(
            true,
            "BiometricRealEngineReadyMessage",
            "Runtime Ready - Local Sample Verification Pending",
            string.Empty,
            openCvVersion,
            paths,
            recognizerMetadata);

    private static RealFaceModelReadiness Failure(
        string errorCode,
        string gateResult,
        string openCvVersion = "")
        => new(
            false,
            "BiometricRealEngineUnavailableMessage",
            gateResult,
            errorCode,
            openCvVersion,
            null);
}
