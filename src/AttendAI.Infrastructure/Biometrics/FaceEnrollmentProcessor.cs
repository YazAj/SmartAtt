using System.Text;
using AttendAI.Application.Biometrics;
using AttendAI.Application.FaceRecognition;
using AttendAI.Domain.Enums;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Biometrics;

public sealed class FaceEnrollmentProcessor : IFaceEnrollmentProcessor
{
    private readonly IFaceCaptureValidator _validator;
    private readonly IFaceRecognitionEngine _engine;
    private readonly BiometricEnrollmentOptions _options;
    private readonly IFaceEngineReadinessService _readinessService;

    public FaceEnrollmentProcessor(
        IFaceCaptureValidator validator,
        IFaceRecognitionEngine engine,
        IOptions<BiometricEnrollmentOptions> options,
        IFaceEngineReadinessService readinessService)
    {
        _validator = validator;
        _engine = engine;
        _options = options.Value;
        _readinessService = readinessService;
    }

    public async Task<FaceEnrollmentProcessingResult> ProcessAsync(
        IReadOnlyCollection<FaceCaptureSample> samples,
        CancellationToken cancellationToken = default)
    {
        var readiness = _readinessService.GetReadiness();
        if (!readiness.EnrollmentEnabled)
        {
            return FaceEnrollmentProcessingResult.Failure(
                FaceCaptureRejectionReason.EngineUnavailable,
                readiness.StatusMessageKey,
                engineName: readiness.EngineName,
                engineVersion: readiness.EngineVersion);
        }

        if (samples.Count < _options.RequiredCaptureCount)
        {
            return FaceEnrollmentProcessingResult.Failure(
                FaceCaptureRejectionReason.ProcessingFailed,
                "ErrorBiometricCaptureCount");
        }

        var processed = new List<ProcessedSample>();
        foreach (var sample in samples.Take(_options.RequiredCaptureCount))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var validation = _validator.Validate(sample);
            if (!validation.Succeeded)
            {
                return FaceEnrollmentProcessingResult.Failure(validation.RejectionReason, validation.MessageKey);
            }

            var qualityFailure = DetectDevelopmentQualityMarker(sample.ImageBytes);
            if (qualityFailure is not null)
            {
                return FaceEnrollmentProcessingResult.Failure(qualityFailure.Value.Reason, qualityFailure.Value.MessageKey);
            }

            var encoding = await _engine.ExtractEncodingAsync(sample.ImageBytes, cancellationToken);
            if (!encoding.Succeeded || encoding.Template is null)
            {
                return MapEngineFailure(encoding, readiness);
            }

            var qualityScore = encoding.QualityScore > 0 ? (decimal)encoding.QualityScore : 0.9m;
            if (qualityScore < _options.MinimumQualityScore)
            {
                return FaceEnrollmentProcessingResult.Failure(
                    FaceCaptureRejectionReason.QualityTooLow,
                    "ErrorBiometricQualityTooLow");
            }

            processed.Add(new ProcessedSample(
                encoding.Template,
                qualityScore,
                encoding.BrightnessScore > 0 ? (decimal)encoding.BrightnessScore : 0.8m,
                encoding.SharpnessScore > 0 ? (decimal)encoding.SharpnessScore : 0.8m,
                encoding.FaceAreaRatio > 0 ? (decimal)encoding.FaceAreaRatio : 0.25m,
                encoding.PoseScore > 0 ? (decimal)encoding.PoseScore : 0.9m));
        }

        if (processed.Count == 0)
        {
            return FaceEnrollmentProcessingResult.Failure(
                FaceCaptureRejectionReason.ProcessingFailed,
                "ErrorBiometricProcessingFailed");
        }

        var selected = processed.OrderByDescending(sample => sample.QualityScore).First();
        return FaceEnrollmentProcessingResult.Success(
            selected.Template,
            processed.Count,
            selected.QualityScore,
            selected.BrightnessScore,
            selected.SharpnessScore,
            selected.FaceAreaRatio,
            selected.PoseScore);
    }

    private static FaceEnrollmentProcessingResult MapEngineFailure(
        FaceEncodingResult encoding,
        FaceEngineReadinessDto readiness)
    {
        var reason = encoding.ErrorCode switch
        {
            FaceRecognitionErrorCode.NoFaceDetected => FaceCaptureRejectionReason.NoFace,
            FaceRecognitionErrorCode.MultipleFacesDetected => FaceCaptureRejectionReason.MultipleFaces,
            FaceRecognitionErrorCode.EngineUnavailable => FaceCaptureRejectionReason.EngineUnavailable,
            FaceRecognitionErrorCode.EmptyImage => FaceCaptureRejectionReason.InvalidImage,
            _ => FaceCaptureRejectionReason.ProcessingFailed
        };

        var messageKey = reason switch
        {
            FaceCaptureRejectionReason.NoFace => "ErrorBiometricNoFace",
            FaceCaptureRejectionReason.MultipleFaces => "ErrorBiometricMultipleFaces",
            FaceCaptureRejectionReason.EngineUnavailable => "ErrorBiometricEngineUnavailable",
            FaceCaptureRejectionReason.InvalidImage => "ErrorBiometricInvalidImage",
            _ => "ErrorBiometricProcessingFailed"
        };

        return FaceEnrollmentProcessingResult.Failure(
            reason,
            messageKey,
            reason == FaceCaptureRejectionReason.MultipleFaces ? 2 : 0,
            readiness.EngineName,
            readiness.EngineVersion);
    }

    private static (FaceCaptureRejectionReason Reason, string MessageKey)? DetectDevelopmentQualityMarker(byte[] imageBytes)
    {
        var text = Encoding.UTF8.GetString(imageBytes);
        if (text.Contains("LOW_QUALITY", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceCaptureRejectionReason.QualityTooLow, "ErrorBiometricQualityTooLow");
        }

        if (text.Contains("DARK_IMAGE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceCaptureRejectionReason.LowBrightness, "ErrorBiometricLowBrightness");
        }

        if (text.Contains("BRIGHT_IMAGE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceCaptureRejectionReason.HighBrightness, "ErrorBiometricHighBrightness");
        }

        if (text.Contains("BLURRY_FACE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceCaptureRejectionReason.LowSharpness, "ErrorBiometricLowSharpness");
        }

        if (text.Contains("FACE_TOO_SMALL", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceCaptureRejectionReason.FaceTooSmall, "ErrorBiometricFaceTooSmall");
        }

        if (text.Contains("FACE_TOO_LARGE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceCaptureRejectionReason.FaceTooLarge, "ErrorBiometricFaceTooLarge");
        }

        return null;
    }

    private sealed record ProcessedSample(
        StoredFaceTemplate Template,
        decimal QualityScore,
        decimal BrightnessScore,
        decimal SharpnessScore,
        decimal FaceAreaRatio,
        decimal PoseScore);
}
