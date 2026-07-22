using AttendAI.Application.FaceVerification;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class TemplateCompatibilityService : ITemplateCompatibilityService
{
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;

    public TemplateCompatibilityService(IFaceEngineDiagnosticsService diagnosticsService)
    {
        _diagnosticsService = diagnosticsService;
    }

    public TemplateCompatibilityResult Check(FaceTemplateCompatibilityMetadata metadata)
    {
        var diagnostics = _diagnosticsService.GetDiagnostics();
        var compatible =
            diagnostics.VerificationEnabled &&
            string.Equals(metadata.EngineName, diagnostics.EngineName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(metadata.EngineVersion, diagnostics.EngineVersion, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(metadata.ModelName, diagnostics.ModelName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(metadata.ModelVersion, diagnostics.ModelVersion, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(metadata.TemplateFormatVersion, diagnostics.ExpectedTemplateFormatVersion, StringComparison.OrdinalIgnoreCase) &&
            metadata.EmbeddingDimension == diagnostics.ExpectedEmbeddingDimension;

        return new TemplateCompatibilityResult(
            compatible,
            compatible ? "FaceVerificationTemplateCompatible" : "ErrorFaceVerificationIncompatibleTemplate",
            diagnostics.EngineName,
            diagnostics.EngineVersion,
            diagnostics.ModelName,
            diagnostics.ModelVersion,
            diagnostics.ExpectedTemplateFormatVersion,
            diagnostics.ExpectedEmbeddingDimension);
    }
}
