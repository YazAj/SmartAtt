using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.FaceVerification;

namespace AttendAI.UnitTests.Application;

public sealed class TemplateCompatibilityServiceTests
{
    [Fact]
    public void Check_accepts_matching_fake_template_metadata()
    {
        var service = new TemplateCompatibilityService(new StubDiagnostics());

        var result = service.Check(new FaceTemplateCompatibilityMetadata(
            "Fake",
            "fake-sha256-v1",
            "Deterministic fake engine",
            "v1",
            "fake-sha256-v1",
            32));

        Assert.True(result.IsCompatible);
    }

    [Fact]
    public void Check_rejects_different_model_version()
    {
        var service = new TemplateCompatibilityService(new StubDiagnostics());

        var result = service.Check(new FaceTemplateCompatibilityMetadata(
            "Fake",
            "fake-sha256-v1",
            "Deterministic fake engine",
            "v2",
            "fake-sha256-v1",
            32));

        Assert.False(result.IsCompatible);
        Assert.Equal("ErrorFaceVerificationIncompatibleTemplate", result.MessageKey);
    }

    private sealed class StubDiagnostics : IFaceEngineDiagnosticsService
    {
        public FaceEngineDiagnosticsDto GetDiagnostics()
            => new(
                "Fake",
                "Fake",
                "fake-sha256-v1",
                "Deterministic fake engine",
                "v1",
                true,
                true,
                true,
                "FaceVerificationFakeEngineDevelopmentMessage",
                "Not Verified",
                0.95m,
                ScoreMetric.CosineSimilarity,
                8,
                15,
                0,
                "fake-sha256-v1",
                32);
    }
}
