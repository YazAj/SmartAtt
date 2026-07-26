using System.Runtime.InteropServices;
using System.Security.Cryptography;
using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;

namespace AttendAI.Infrastructure.FaceRecognition;

public sealed class OpenCvSFaceRecognitionEngine : IFaceRecognitionEngine, IDisposable
{
    private readonly RealFaceRecognitionModelStore _modelStore;
    private readonly RealFaceRecognitionOptions _options;
    private readonly ILogger<OpenCvSFaceRecognitionEngine> _logger;
    private readonly object _sessionLock = new();
    private InferenceSession? _recognizerSession;
    private string? _inputName;
    private string? _outputName;
    private bool _disposed;

    public OpenCvSFaceRecognitionEngine(
        RealFaceRecognitionModelStore modelStore,
        IOptions<FaceRecognitionOptions> options,
        ILogger<OpenCvSFaceRecognitionEngine> logger)
    {
        _modelStore = modelStore;
        _options = options.Value.RealEngine;
        _logger = logger;
    }

    public Task<FaceEncodingResult> ExtractEncodingAsync(
        byte[] imageBytes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (imageBytes.Length == 0)
        {
            return Task.FromResult(FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.EmptyImage,
                "The submitted image payload is empty."));
        }

        try
        {
            return Task.FromResult(ProcessImage(imageBytes, cancellationToken));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception) when (IsSafeNativeFailure(exception))
        {
            _logger.LogWarning(exception, "Real face-recognition extraction failed safely.");
            return Task.FromResult(FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.EngineUnavailable,
                "The real face-recognition engine could not process the image."));
        }
    }

    public async Task<FaceVerificationResult> VerifyAsync(
        byte[] imageBytes,
        StoredFaceTemplate template,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsCompatibleTemplate(template))
        {
            return FaceVerificationResult.Failure(
                FaceRecognitionErrorCode.TemplateUnavailable,
                "The stored face template is not compatible with the configured real engine.");
        }

        float[]? storedEmbedding = null;
        float[]? currentEmbedding = null;
        byte[]? currentTemplateBytes = null;
        try
        {
            storedEmbedding = SFaceTemplateCodec.Deserialize(
                template.TemplateData,
                Math.Max(1, _options.EmbeddingDimension));

            var currentEncoding = await ExtractEncodingAsync(imageBytes, cancellationToken);
            if (!currentEncoding.Succeeded || currentEncoding.Template is null)
            {
                return FaceVerificationResult.Failure(currentEncoding.ErrorCode, currentEncoding.Message);
            }

            currentTemplateBytes = currentEncoding.Template.TemplateData;
            currentEmbedding = SFaceTemplateCodec.Deserialize(
                currentTemplateBytes,
                Math.Max(1, _options.EmbeddingDimension));

            var similarity = RealFaceRecognitionModelStore.CosineSimilarity(currentEmbedding, storedEmbedding);
            return similarity >= _options.CosineSimilarityThreshold
                ? FaceVerificationResult.Match(similarity)
                : FaceVerificationResult.NoMatch(similarity);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Real face-recognition verification rejected an invalid template or embedding.");
            return FaceVerificationResult.Failure(
                FaceRecognitionErrorCode.InvalidEmbedding,
                "The stored face template could not be interpreted by the configured real engine.");
        }
        finally
        {
            if (storedEmbedding is not null)
            {
                CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(storedEmbedding.AsSpan()));
            }

            if (currentEmbedding is not null)
            {
                CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(currentEmbedding.AsSpan()));
            }

            if (currentTemplateBytes is not null)
            {
                CryptographicOperations.ZeroMemory(currentTemplateBytes);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _recognizerSession?.Dispose();
        _disposed = true;
    }

    private FaceEncodingResult ProcessImage(byte[] imageBytes, CancellationToken cancellationToken)
    {
        using var image = Cv2.ImDecode(imageBytes, ImreadModes.Color);
        if (image.Empty() || image.Width <= 0 || image.Height <= 0)
        {
            return FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.InvalidImage,
                "The submitted image could not be decoded.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var readiness = _modelStore.CheckReadiness(loadNativeModels: false);
        if (!readiness.Succeeded || readiness.Paths is null)
        {
            return FaceEncodingResult.Failure(
                FaceRecognitionErrorCode.EngineUnavailable,
                "The real face-recognition models are not ready.");
        }

        var brightnessScore = CalculateBrightnessScore(image);
        if (brightnessScore < _options.MinimumBrightnessScore)
        {
            return Failure(FaceRecognitionErrorCode.LowBrightness, "The capture is too dark.");
        }

        if (brightnessScore > _options.MaximumBrightnessScore)
        {
            return Failure(FaceRecognitionErrorCode.HighBrightness, "The capture is too bright.");
        }

        var sharpnessScore = CalculateSharpnessScore(image);
        if (sharpnessScore < _options.MinimumSharpnessScore)
        {
            return Failure(FaceRecognitionErrorCode.LowSharpness, "The capture is too blurry.");
        }

        var detection = DetectSingleFace(image, readiness.Paths);
        if (!detection.Succeeded || detection.Face is null)
        {
            return new FaceEncodingResult(
                false,
                null,
                detection.ErrorCode,
                detection.Message,
                detection.FaceCount);
        }

        cancellationToken.ThrowIfCancellationRequested();

        using var alignedFace = AlignFace(image, detection.Face.Landmarks);
        if (alignedFace.Empty() ||
            alignedFace.Width != RealFaceRecognitionMetadata.AlignedFaceWidth ||
            alignedFace.Height != RealFaceRecognitionMetadata.AlignedFaceHeight)
        {
            return Failure(FaceRecognitionErrorCode.InvalidLandmarks, "Face alignment failed safely.");
        }

        var embedding = ExtractEmbedding(alignedFace, readiness.Paths.RecognizerModelPath);
        if (embedding.Length != _options.EmbeddingDimension)
        {
            CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(embedding.AsSpan()));
            return Failure(FaceRecognitionErrorCode.InvalidEmbedding, "SFace embedding dimension is invalid.");
        }

        byte[] templateData;
        try
        {
            templateData = SFaceTemplateCodec.Serialize(embedding);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(embedding.AsSpan()));
        }

        var qualityScore = CalculateQualityScore(
            detection.Face.Confidence,
            brightnessScore,
            sharpnessScore,
            detection.Face.FaceAreaRatio);

        var template = new StoredFaceTemplate(
            _options.TemplateFormatVersion,
            RealFaceRecognitionMetadata.TemplateFormat,
            templateData,
            DateTimeOffset.UtcNow,
            _options.EngineName,
            _options.EngineVersion,
            _options.ModelName,
            _options.ModelVersion,
            _options.EmbeddingDimension,
            qualityScore);

        return new FaceEncodingResult(
            true,
            template,
            FaceRecognitionErrorCode.None,
            "Real face template extracted.",
            1,
            qualityScore,
            brightnessScore,
            sharpnessScore,
            detection.Face.FaceAreaRatio,
            detection.Face.Confidence,
            template.EngineName,
            template.EngineVersion,
            template.ModelName,
            template.ModelVersion);

        static FaceEncodingResult Failure(FaceRecognitionErrorCode code, string message)
            => FaceEncodingResult.Failure(code, message);
    }

    private YuNetDetectionInterpretation DetectSingleFace(Mat image, RealFaceModelPaths paths)
    {
        using var detector = FaceDetectorYN.Create(
            paths.DetectorModelPath,
            string.Empty,
            new Size(image.Width, image.Height),
            (float)Math.Clamp(_options.DetectionScoreThreshold, 0.01, 0.99),
            (float)Math.Clamp(_options.DetectionNmsThreshold, 0.01, 0.99),
            Math.Max(1, _options.DetectionTopK),
            Backend.OPENCV,
            Target.CPU);

        using var faces = new Mat();
        detector.Detect(image, faces);

        var rows = new List<float[]>();
        if (!faces.Empty())
        {
            for (var row = 0; row < faces.Rows; row++)
            {
                var values = new float[faces.Cols];
                for (var col = 0; col < faces.Cols; col++)
                {
                    values[col] = faces.At<float>(row, col);
                }

                rows.Add(values);
            }
        }

        return YuNetDetectionInterpreter.Interpret(rows, image.Width, image.Height, _options);
    }

    private Mat AlignFace(Mat image, IReadOnlyList<Point2f> landmarks)
    {
        var transformData = BuildSimilarityTransform(landmarks);
        using var transform = new Mat(2, 3, MatType.CV_64FC1);
        transform.SetArray(transformData);

        var aligned = new Mat();
        Cv2.WarpAffine(
            image,
            aligned,
            transform,
            new Size(RealFaceRecognitionMetadata.AlignedFaceWidth, RealFaceRecognitionMetadata.AlignedFaceHeight),
            InterpolationFlags.Linear,
            BorderTypes.Constant,
            new Scalar(0, 0, 0));

        return aligned;
    }

    private float[] ExtractEmbedding(Mat alignedFace, string recognizerModelPath)
    {
        var session = EnsureRecognizerSession(recognizerModelPath);
        using var blob = CvDnn.BlobFromImage(
            alignedFace,
            1.0,
            new Size(RealFaceRecognitionMetadata.AlignedFaceWidth, RealFaceRecognitionMetadata.AlignedFaceHeight),
            new Scalar(0, 0, 0),
            swapRB: true,
            crop: false);

        var inputData = new float[(int)blob.Total()];
        Marshal.Copy(blob.Data, inputData, 0, inputData.Length);

        var input = new DenseTensor<float>(
            inputData,
            new[]
            {
                1,
                3,
                RealFaceRecognitionMetadata.AlignedFaceHeight,
                RealFaceRecognitionMetadata.AlignedFaceWidth
            });

        try
        {
            using var results = session.Run([NamedOnnxValue.CreateFromTensor(_inputName!, input)]);
            var output = results.First(result => string.Equals(result.Name, _outputName, StringComparison.Ordinal));
            return output.AsTensor<float>().ToArray();
        }
        finally
        {
            CryptographicOperations.ZeroMemory(MemoryMarshal.AsBytes(inputData.AsSpan()));
        }
    }

    private InferenceSession EnsureRecognizerSession(string recognizerModelPath)
    {
        if (_recognizerSession is not null)
        {
            return _recognizerSession;
        }

        lock (_sessionLock)
        {
            if (_recognizerSession is not null)
            {
                return _recognizerSession;
            }

            var session = RealFaceRecognitionModelStore.CreateRecognizerSession(recognizerModelPath);
            var inspection = RealFaceRecognitionModelStore.InspectRecognizerSession(session);
            if (!inspection.Succeeded)
            {
                session.Dispose();
                throw new InvalidOperationException(inspection.Message);
            }

            _inputName = session.InputMetadata.Keys.Single();
            _outputName = session.OutputMetadata.Keys.First();
            _recognizerSession = session;
            return _recognizerSession;
        }
    }

    private bool IsCompatibleTemplate(StoredFaceTemplate template)
        => string.Equals(template.EngineName, _options.EngineName, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(template.EngineVersion, _options.EngineVersion, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(template.ModelName, _options.ModelName, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(template.ModelVersion, _options.ModelVersion, StringComparison.OrdinalIgnoreCase) &&
           string.Equals(template.Version, _options.TemplateFormatVersion, StringComparison.OrdinalIgnoreCase) &&
           template.EncodingDimension == _options.EmbeddingDimension &&
           template.TemplateData.Length == _options.EmbeddingDimension * sizeof(float);

    private static double[] BuildSimilarityTransform(IReadOnlyList<Point2f> source)
    {
        var destination = new[]
        {
            new Point2d(38.2946, 51.6963),
            new Point2d(73.5318, 51.5014),
            new Point2d(56.0252, 71.7366),
            new Point2d(41.5493, 92.3655),
            new Point2d(70.7299, 92.2041)
        };

        var sourceMean = new Point2d(source.Average(point => point.X), source.Average(point => point.Y));
        var destinationMean = new Point2d(56.0262, 71.9008);
        var sourceDemean = new Point2d[5];
        var destinationDemean = new Point2d[5];
        for (var index = 0; index < 5; index++)
        {
            sourceDemean[index] = new Point2d(
                source[index].X - sourceMean.X,
                source[index].Y - sourceMean.Y);
            destinationDemean[index] = new Point2d(
                destination[index].X - destinationMean.X,
                destination[index].Y - destinationMean.Y);
        }

        double a00 = 0;
        double a01 = 0;
        double a10 = 0;
        double a11 = 0;
        for (var index = 0; index < 5; index++)
        {
            a00 += destinationDemean[index].X * sourceDemean[index].X;
            a01 += destinationDemean[index].X * sourceDemean[index].Y;
            a10 += destinationDemean[index].Y * sourceDemean[index].X;
            a11 += destinationDemean[index].Y * sourceDemean[index].Y;
        }

        a00 /= 5;
        a01 /= 5;
        a10 /= 5;
        a11 /= 5;

        var d0 = 1d;
        var d1 = (a00 * a11) - (a01 * a10) < 0 ? -1d : 1d;

        using var a = new Mat(2, 2, MatType.CV_64FC1);
        a.SetArray(new[] { a00, a01, a10, a11 });
        using var s = new Mat();
        using var u = new Mat();
        using var vt = new Mat();
        Cv2.SVDecomp(a, s, u, vt);

        var s0 = s.At<double>(0);
        var s1 = s.At<double>(1);
        var smax = Math.Max(s0, s1);
        var tolerance = smax * 2 * float.Epsilon;
        var rank = 0;
        if (s0 > tolerance)
        {
            rank++;
        }

        if (s1 > tolerance)
        {
            rank++;
        }

        var uArray = new[,]
        {
            { u.At<double>(0, 0), u.At<double>(0, 1) },
            { u.At<double>(1, 0), u.At<double>(1, 1) }
        };
        var vtArray = new[,]
        {
            { vt.At<double>(0, 0), vt.At<double>(0, 1) },
            { vt.At<double>(1, 0), vt.At<double>(1, 1) }
        };

        var detU = (uArray[0, 0] * uArray[1, 1]) - (uArray[0, 1] * uArray[1, 0]);
        var detVt = (vtArray[0, 0] * vtArray[1, 1]) - (vtArray[0, 1] * vtArray[1, 0]);

        double[,] rotation;
        if (rank == 1 && (detU * detVt) > 0)
        {
            rotation = Multiply(uArray, vtArray);
        }
        else
        {
            var dForRotation = rank == 1 ? -d1 : d1;
            rotation = Multiply(Multiply(uArray, new[,] { { d0, 0d }, { 0d, dForRotation } }), vtArray);
        }

        double varianceX = 0;
        double varianceY = 0;
        for (var index = 0; index < 5; index++)
        {
            varianceX += sourceDemean[index].X * sourceDemean[index].X;
            varianceY += sourceDemean[index].Y * sourceDemean[index].Y;
        }

        varianceX /= 5;
        varianceY /= 5;
        var variance = varianceX + varianceY;
        if (variance <= 0)
        {
            throw new InvalidOperationException("Face landmarks cannot define an alignment transform.");
        }

        var scale = ((s0 * d0) + (s1 * d1)) / variance;
        var transformedSourceMeanX = (rotation[0, 0] * sourceMean.X) + (rotation[0, 1] * sourceMean.Y);
        var transformedSourceMeanY = (rotation[1, 0] * sourceMean.X) + (rotation[1, 1] * sourceMean.Y);
        var tx = destinationMean.X - (scale * transformedSourceMeanX);
        var ty = destinationMean.Y - (scale * transformedSourceMeanY);

        return
        [
            rotation[0, 0] * scale,
            rotation[0, 1] * scale,
            tx,
            rotation[1, 0] * scale,
            rotation[1, 1] * scale,
            ty
        ];
    }

    private static double[,] Multiply(double[,] left, double[,] right)
        =>
        new[,]
        {
            {
                (left[0, 0] * right[0, 0]) + (left[0, 1] * right[1, 0]),
                (left[0, 0] * right[0, 1]) + (left[0, 1] * right[1, 1])
            },
            {
                (left[1, 0] * right[0, 0]) + (left[1, 1] * right[1, 0]),
                (left[1, 0] * right[0, 1]) + (left[1, 1] * right[1, 1])
            }
        };

    private static double CalculateBrightnessScore(Mat image)
    {
        using var gray = new Mat();
        Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        return Math.Clamp(Cv2.Mean(gray).Val0 / 255d, 0d, 1d);
    }

    private static double CalculateSharpnessScore(Mat image)
    {
        using var gray = new Mat();
        using var laplacian = new Mat();
        Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Laplacian(gray, laplacian, MatType.CV_64F);
        Cv2.MeanStdDev(laplacian, out _, out var stddev);
        var variance = stddev.Val0 * stddev.Val0;
        return Math.Clamp(variance / 1000d, 0d, 1d);
    }

    private static double CalculateQualityScore(
        double confidence,
        double brightness,
        double sharpness,
        double faceAreaRatio)
    {
        var brightnessQuality = 1d - Math.Min(1d, Math.Abs(brightness - 0.55d) / 0.55d);
        var areaQuality = Math.Clamp(faceAreaRatio / 0.12d, 0d, 1d);
        return Math.Clamp(
            (confidence * 0.35d) +
            (brightnessQuality * 0.25d) +
            (sharpness * 0.25d) +
            (areaQuality * 0.15d),
            0d,
            1d);
    }

    private static bool IsSafeNativeFailure(Exception exception)
        => exception is OpenCVException or OnnxRuntimeException or DllNotFoundException or BadImageFormatException or InvalidOperationException;

}
