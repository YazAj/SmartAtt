using AttendAI.Application.Biometrics;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Biometrics;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AttendAI.UnitTests.Application;

public sealed class FaceEnrollmentProcessorTests
{
    [Fact]
    public async Task ProcessAsync_creates_template_from_required_fake_captures()
    {
        var processor = CreateProcessor();
        var captures = new[]
        {
            Sample(),
            Sample(),
            Sample()
        };

        var result = await processor.ProcessAsync(captures);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Template);
        Assert.Equal(3, result.CaptureCount);
        Assert.Equal("Fake", result.EngineName);
    }

    [Fact]
    public async Task ProcessAsync_rejects_no_face_marker()
    {
        var processor = CreateProcessor();
        var captures = new[]
        {
            Sample("NO_FACE"),
            Sample(),
            Sample()
        };

        var result = await processor.ProcessAsync(captures);

        Assert.False(result.Succeeded);
        Assert.Equal(FaceCaptureRejectionReason.NoFace, result.RejectionReason);
    }

    [Fact]
    public async Task ProcessAsync_rejects_development_low_quality_marker()
    {
        var processor = CreateProcessor();
        var captures = new[]
        {
            Sample("LOW_QUALITY"),
            Sample(),
            Sample()
        };

        var result = await processor.ProcessAsync(captures);

        Assert.False(result.Succeeded);
        Assert.Equal(FaceCaptureRejectionReason.QualityTooLow, result.RejectionReason);
    }

    private static FaceEnrollmentProcessor CreateProcessor()
    {
        var biometricOptions = Options.Create(new BiometricEnrollmentOptions());
        var faceOptions = Options.Create(new FaceRecognitionOptions
        {
            Provider = "Fake",
            EngineVersion = "fake-sha256-v1",
            ModelName = "Deterministic fake engine",
            ModelVersion = "v1"
        });
        var readiness = new FaceEngineReadinessService(faceOptions, new TestHostEnvironment());

        return new FaceEnrollmentProcessor(
            new FaceCaptureValidator(biometricOptions),
            new FakeFaceRecognitionEngine(faceOptions),
            biometricOptions,
            readiness);
    }

    private static FaceCaptureSample Sample(string? marker = null)
        => new(BiometricCaptureValidatorTests.Png(160, 120, marker), "image/png", "captures");

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "AttendAI.UnitTests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
