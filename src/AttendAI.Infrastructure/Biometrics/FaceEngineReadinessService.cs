using AttendAI.Application.Biometrics;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Biometrics;

public sealed class FaceEngineReadinessService : IFaceEngineReadinessService
{
    private readonly FaceRecognitionOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly RealFaceRecognitionModelStore _modelStore;

    public FaceEngineReadinessService(
        IOptions<FaceRecognitionOptions> options,
        IHostEnvironment environment,
        RealFaceRecognitionModelStore modelStore)
    {
        _options = options.Value;
        _environment = environment;
        _modelStore = modelStore;
    }

    public FaceEngineReadinessDto GetReadiness()
    {
        var mode = string.IsNullOrWhiteSpace(_options.Provider) ? "Fake" : _options.Provider.Trim();
        var normalized = mode.ToLowerInvariant();
        var engineName = normalized == "fake" ? "Fake" : normalized == "disabled" ? "Disabled" : "Real";
        var engineVersion = string.IsNullOrWhiteSpace(_options.EngineVersion) ? "Not configured" : _options.EngineVersion;
        var modelName = string.IsNullOrWhiteSpace(_options.ModelName) ? "Not configured" : _options.ModelName;
        var modelVersion = string.IsNullOrWhiteSpace(_options.ModelVersion) ? "Not configured" : _options.ModelVersion;

        if (normalized == "disabled")
        {
            return new FaceEngineReadinessDto(
                "Disabled",
                engineName,
                engineVersion,
                modelName,
                modelVersion,
                false,
                false,
                "BiometricEngineDisabledMessage",
                "Not Verified");
        }

        if (normalized == "real")
        {
            var real = _options.RealEngine;
            var readiness = _modelStore.CheckReadiness();
            return new FaceEngineReadinessDto(
                "Real",
                string.IsNullOrWhiteSpace(real.EngineName) ? engineName : real.EngineName,
                string.IsNullOrWhiteSpace(real.EngineVersion) ? engineVersion : real.EngineVersion,
                string.IsNullOrWhiteSpace(real.ModelName) ? modelName : real.ModelName,
                string.IsNullOrWhiteSpace(real.ModelVersion) ? modelVersion : real.ModelVersion,
                readiness.Succeeded,
                !_environment.IsProduction() && readiness.Succeeded,
                readiness.StatusMessageKey,
                readiness.GateResult);
        }

        var productionSafe = !_environment.IsProduction();
        return new FaceEngineReadinessDto(
            "Fake",
            engineName,
            engineVersion,
            modelName,
            modelVersion,
            productionSafe,
            productionSafe,
            productionSafe ? "BiometricFakeEngineDevelopmentMessage" : "BiometricFakeEngineProductionBlockedMessage",
            "Not Verified");
    }
}
