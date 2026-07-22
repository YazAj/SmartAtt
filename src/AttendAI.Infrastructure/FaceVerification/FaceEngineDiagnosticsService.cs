using AttendAI.Application.Biometrics;
using AttendAI.Application.FaceVerification;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class FaceEngineDiagnosticsService : IFaceEngineDiagnosticsService
{
    private readonly IFaceEngineReadinessService _readinessService;
    private readonly FaceVerificationOptions _verificationOptions;
    private readonly IHostEnvironment _environment;

    public FaceEngineDiagnosticsService(
        IFaceEngineReadinessService readinessService,
        IOptions<FaceVerificationOptions> verificationOptions,
        IHostEnvironment environment)
    {
        _readinessService = readinessService;
        _verificationOptions = verificationOptions.Value;
        _environment = environment;
    }

    public FaceEngineDiagnosticsDto GetDiagnostics()
    {
        var readiness = _readinessService.GetReadiness();
        var mode = readiness.Mode;
        var threshold = _verificationOptions.VerificationThreshold <= 0 ? 0.95m : (decimal)_verificationOptions.VerificationThreshold;

        var modeAllowsVerification = mode switch
        {
            "Disabled" => false,
            "Real" => false,
            "Fake" => !_environment.IsProduction() &&
                _verificationOptions.AllowFakeEngineInDevelopment &&
                !_verificationOptions.RequireRealEngineForVerification,
            _ => false
        };
        var enabled = _verificationOptions.FaceVerificationEnabled && modeAllowsVerification;

        var messageKey = !_verificationOptions.FaceVerificationEnabled
            ? "FaceVerificationDisabledMessage"
            : mode == "Disabled"
                ? "FaceVerificationEngineDisabledMessage"
                : mode == "Real"
                    ? "FaceVerificationRealEngineUnavailableMessage"
                    : enabled
                        ? "FaceVerificationFakeEngineDevelopmentMessage"
                        : "FaceVerificationFakeEngineBlockedMessage";

        return new FaceEngineDiagnosticsDto(
            mode,
            readiness.EngineName,
            readiness.EngineVersion,
            readiness.ModelName,
            readiness.ModelVersion,
            enabled,
            mode == "Fake" && enabled,
            readiness.IsProductionSafe,
            messageKey,
            readiness.GateResult,
            threshold,
            _verificationOptions.ScoreMetric,
            Math.Max(1, _verificationOptions.MaximumAttemptsPerWindow),
            Math.Max(1, _verificationOptions.AttemptWindowMinutes),
            Math.Max(0, _verificationOptions.CooldownSeconds),
            string.IsNullOrWhiteSpace(_verificationOptions.ExpectedTemplateFormatVersion)
                ? readiness.EngineVersion
                : _verificationOptions.ExpectedTemplateFormatVersion,
            Math.Max(1, _verificationOptions.ExpectedEmbeddingDimension));
    }
}
