using System.Buffers.Binary;
using System.Security.Cryptography;
using AttendAI.Application.FaceRecognition;
using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using AttendAI.Infrastructure.FaceVerification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AttendAI.UnitTests.Application;

public sealed class RealFaceRecognitionEngineTests
{
    [Fact]
    public void ResolvePaths_rejects_model_path_traversal()
    {
        using var scope = TempScope.Create();
        var options = Options.Create(OptionsFor(scope.Root));
        options.Value.RealEngine.DetectorModelPath = "../outside/yunet.onnx";

        var store = new RealFaceRecognitionModelStore(options, scope.Environment);

        Assert.Throws<InvalidOperationException>(() => store.ResolvePaths());
    }

    [Fact]
    public void CheckReadiness_reports_missing_detector_model()
    {
        using var scope = TempScope.Create();
        var store = Store(scope, OptionsFor(scope.Root));

        var readiness = store.CheckReadiness();

        Assert.False(readiness.Succeeded);
        Assert.Equal("DetectorMissing", readiness.ErrorCode);
    }

    [Fact]
    public void CheckReadiness_reports_missing_recognizer_model()
    {
        using var scope = TempScope.Create();
        var options = OptionsFor(scope.Root);
        var detectorBytes = Bytes(150_000, 17);
        WriteModel(scope, options.RealEngine.DetectorModelPath, detectorBytes);
        options.RealEngine.DetectorModelSha256 = Sha256(detectorBytes);

        var readiness = Store(scope, options).CheckReadiness();

        Assert.False(readiness.Succeeded);
        Assert.Equal("RecognizerMissing", readiness.ErrorCode);
    }

    [Fact]
    public void CheckReadiness_rejects_empty_model_file()
    {
        using var scope = TempScope.Create();
        var options = OptionsFor(scope.Root);
        WriteModel(scope, options.RealEngine.DetectorModelPath, []);
        options.RealEngine.DetectorModelSha256 = Sha256([]);

        var readiness = Store(scope, options).CheckReadiness();

        Assert.False(readiness.Succeeded);
        Assert.Equal("DetectorTooSmall", readiness.ErrorCode);
    }

    [Fact]
    public void CheckReadiness_rejects_git_lfs_pointer_file()
    {
        using var scope = TempScope.Create();
        var options = OptionsFor(scope.Root);
        var pointerBytes = "version https://git-lfs.github.com/spec/v1\n"u8.ToArray();
        WriteModel(scope, options.RealEngine.DetectorModelPath, pointerBytes);
        options.RealEngine.DetectorModelSha256 = Sha256(pointerBytes);

        var readiness = Store(scope, options).CheckReadiness();

        Assert.False(readiness.Succeeded);
        Assert.Equal("DetectorGitLfsPointer", readiness.ErrorCode);
    }

    [Fact]
    public void CheckReadiness_rejects_incorrect_hash()
    {
        using var scope = TempScope.Create();
        var options = OptionsFor(scope.Root);
        WriteModel(scope, options.RealEngine.DetectorModelPath, Bytes(150_000, 23));
        options.RealEngine.DetectorModelSha256 = new string('0', 64);

        var readiness = Store(scope, options).CheckReadiness();

        Assert.False(readiness.Succeeded);
        Assert.Equal("DetectorHashMismatch", readiness.ErrorCode);
    }

    [Fact]
    public void CheckReadiness_maps_invalid_onnx_to_safe_failure()
    {
        using var scope = TempScope.Create();
        var options = OptionsFor(scope.Root);
        var detectorBytes = Bytes(150_000, 29);
        var recognizerBytes = Bytes(1_100_000, 31);
        WriteModel(scope, options.RealEngine.DetectorModelPath, detectorBytes);
        WriteModel(scope, options.RealEngine.RecognizerModelPath, recognizerBytes);
        options.RealEngine.DetectorModelSha256 = Sha256(detectorBytes);
        options.RealEngine.RecognizerModelSha256 = Sha256(recognizerBytes);

        var readiness = Store(scope, options).CheckReadiness();

        Assert.False(readiness.Succeeded);
        Assert.Contains("LoadFailed", readiness.ErrorCode, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Real_engine_returns_empty_image_before_model_readiness()
    {
        using var scope = TempScope.Create();
        var engine = Engine(scope, OptionsFor(scope.Root));

        var result = await engine.ExtractEncodingAsync([]);

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.EmptyImage, result.ErrorCode);
    }

    [Fact]
    public async Task Real_engine_returns_invalid_image_before_model_readiness()
    {
        using var scope = TempScope.Create();
        var engine = Engine(scope, OptionsFor(scope.Root));

        var result = await engine.ExtractEncodingAsync("not an image"u8.ToArray());

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.InvalidImage, result.ErrorCode);
    }

    [Fact]
    public void YuNet_interpreter_returns_no_face_for_empty_rows()
    {
        var result = YuNetDetectionInterpreter.Interpret([], 640, 480, new RealFaceRecognitionOptions());

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.NoFaceDetected, result.ErrorCode);
    }

    [Fact]
    public void YuNet_interpreter_returns_multiple_faces_for_two_accepted_rows()
    {
        var options = new RealFaceRecognitionOptions();
        var result = YuNetDetectionInterpreter.Interpret(
            [FaceRow(40, 40), FaceRow(180, 40)],
            640,
            480,
            options);

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.MultipleFacesDetected, result.ErrorCode);
        Assert.Equal(2, result.FaceCount);
    }

    [Fact]
    public void YuNet_interpreter_rejects_invalid_landmarks()
    {
        var row = FaceRow(40, 40);
        row[4] = -5;

        var result = YuNetDetectionInterpreter.Interpret([row], 640, 480, new RealFaceRecognitionOptions());

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.InvalidLandmarks, result.ErrorCode);
    }

    [Fact]
    public void YuNet_interpreter_rejects_too_small_face()
    {
        var options = new RealFaceRecognitionOptions { MinimumFaceAreaRatio = 0.10 };

        var result = YuNetDetectionInterpreter.Interpret([FaceRow(40, 40)], 640, 480, options);

        Assert.False(result.Succeeded);
        Assert.Equal(FaceRecognitionErrorCode.FaceTooSmall, result.ErrorCode);
    }

    [Fact]
    public void SFace_template_codec_round_trips_little_endian_normalized_embedding()
    {
        var bytes = SFaceTemplateCodec.Serialize([3f, 4f]);

        var first = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(0, sizeof(float)));
        var second = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(sizeof(float), sizeof(float)));
        var embedding = SFaceTemplateCodec.Deserialize(bytes, 2);

        Assert.Equal(0.6f, first, precision: 6);
        Assert.Equal(0.8f, second, precision: 6);
        Assert.Equal(0.6f, embedding[0], precision: 6);
        Assert.Equal(0.8f, embedding[1], precision: 6);
    }

    [Fact]
    public void SFace_template_codec_rejects_empty_embedding()
        => Assert.Throws<InvalidOperationException>(() => SFaceTemplateCodec.Serialize([]));

    [Fact]
    public void SFace_template_codec_rejects_non_finite_embedding()
        => Assert.Throws<InvalidOperationException>(() => SFaceTemplateCodec.Serialize([float.NaN]));

    [Fact]
    public void SFace_template_codec_rejects_corrupt_payload()
        => Assert.Throws<InvalidOperationException>(() => SFaceTemplateCodec.Deserialize([1, 2, 3], 1));

    [Fact]
    public void SFace_template_codec_rejects_dimension_mismatch()
    {
        var bytes = SFaceTemplateCodec.Serialize([1f, 0f]);

        Assert.Throws<InvalidOperationException>(() => SFaceTemplateCodec.Deserialize(bytes, 3));
    }

    [Fact]
    public void Cosine_similarity_returns_one_for_identical_normalized_embeddings()
    {
        var score = RealFaceRecognitionModelStore.CosineSimilarity([0.6f, 0.8f], [0.6f, 0.8f]);

        Assert.Equal(1d, score, precision: 6);
    }

    [Fact]
    public void Threshold_policy_matches_on_equality_for_cosine_similarity()
    {
        var policy = new FaceVerificationPolicyService(Options.Create(new FaceVerificationOptions()));

        Assert.Equal(
            FaceVerificationDecision.Match,
            policy.Decide(0.363m, 0.363m, ScoreMetric.CosineSimilarity));
    }

    [Fact]
    public void Template_compatibility_rejects_fake_template_in_real_mode()
    {
        var service = new TemplateCompatibilityService(new RealDiagnostics());

        var result = service.Check(new FaceTemplateCompatibilityMetadata(
            "Fake",
            "fake-sha256-v1",
            "Deterministic fake engine",
            "v1",
            "fake-sha256-v1",
            32));

        Assert.False(result.IsCompatible);
        Assert.Equal("ErrorFaceVerificationIncompatibleTemplate", result.MessageKey);
    }

    [Fact]
    public void Template_compatibility_accepts_real_template_metadata()
    {
        var service = new TemplateCompatibilityService(new RealDiagnostics());

        var result = service.Check(new FaceTemplateCompatibilityMetadata(
            "OpenCV-SFace",
            "OpenCvSharp4 4.13.0.20260627 / OpenCV 4.13.0",
            "SFace",
            "2021dec",
            "opencv-sface-f32le-v1",
            128));

        Assert.True(result.IsCompatible);
    }

    [Theory]
    [InlineData("Fake", typeof(FakeFaceRecognitionEngine))]
    [InlineData("Real", typeof(OpenCvSFaceRecognitionEngine))]
    [InlineData("Disabled", typeof(DisabledFaceRecognitionEngine))]
    public void Dependency_injection_selects_exactly_one_face_engine_for_mode(
        string provider,
        Type expectedType)
    {
        using var scope = TempScope.Create();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FaceRecognition:Provider"] = provider,
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=AttendAI_UnitTests;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(scope.Environment);
        services.AddLogging();
        services.AddInfrastructureServices(configuration);
        using var providerRoot = services.BuildServiceProvider();

        var engine = providerRoot.GetRequiredService<IFaceRecognitionEngine>();

        Assert.IsType(expectedType, engine);
    }

    private static OpenCvSFaceRecognitionEngine Engine(TempScope scope, FaceRecognitionOptions options)
    {
        var wrapped = Options.Create(options);
        return new OpenCvSFaceRecognitionEngine(
            new RealFaceRecognitionModelStore(wrapped, scope.Environment),
            wrapped,
            NullLogger<OpenCvSFaceRecognitionEngine>.Instance);
    }

    private static RealFaceRecognitionModelStore Store(TempScope scope, FaceRecognitionOptions options)
        => new(Options.Create(options), scope.Environment);

    private static FaceRecognitionOptions OptionsFor(string root)
        => new()
        {
            Provider = "Real",
            RealEngine = new RealFaceRecognitionOptions
            {
                ApprovedModelRoot = "models/face-recognition",
                DetectorModelPath = "models/face-recognition/yunet/face_detection_yunet_2023mar.onnx",
                RecognizerModelPath = "models/face-recognition/sface/face_recognition_sface_2021dec.onnx"
            }
        };

    private static void WriteModel(TempScope scope, string relativePath, byte[] bytes)
    {
        var fullPath = Path.Combine(scope.Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllBytes(fullPath, bytes);
    }

    private static byte[] Bytes(int length, byte seed)
    {
        var bytes = new byte[length];
        for (var index = 0; index < bytes.Length; index++)
        {
            bytes[index] = (byte)(seed + index);
        }

        return bytes;
    }

    private static string Sha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static float[] FaceRow(float x, float y)
        =>
        [
            x, y, 120, 140,
            x + 35, y + 50,
            x + 85, y + 50,
            x + 60, y + 82,
            x + 42, y + 112,
            x + 78, y + 112,
            0.95f
        ];

    private sealed class RealDiagnostics : IFaceEngineDiagnosticsService
    {
        public FaceEngineDiagnosticsDto GetDiagnostics()
            => new(
                "Real",
                "OpenCV-SFace",
                "OpenCvSharp4 4.13.0.20260627 / OpenCV 4.13.0",
                "SFace",
                "2021dec",
                true,
                false,
                true,
                "FaceVerificationReadyMessage",
                "Runtime Ready - Local Sample Verification Pending",
                0.363m,
                ScoreMetric.CosineSimilarity,
                8,
                15,
                0,
                "opencv-sface-f32le-v1",
                128);
    }

    private sealed class TempScope : IDisposable
    {
        private TempScope(string root)
        {
            Root = root;
            Environment = new TestHostEnvironment(root);
        }

        public string Root { get; }

        public TestHostEnvironment Environment { get; }

        public static TempScope Create()
            => new(Path.Combine(Path.GetTempPath(), "AttendAI.RealFaceTests", Guid.NewGuid().ToString("N")));

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public TestHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }

        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "AttendAI.UnitTests";

        public string ContentRootPath { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
