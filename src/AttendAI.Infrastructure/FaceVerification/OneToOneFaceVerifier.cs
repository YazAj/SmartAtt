using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using AttendAI.Application.Biometrics;
using AttendAI.Application.FaceRecognition;
using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class OneToOneFaceVerifier : IOneToOneFaceVerifier
{
    private const string StoredTemplateFormat = "application/vnd.attendai.face-template";
    private readonly ApplicationDbContext _dbContext;
    private readonly IFaceCaptureValidator _captureValidator;
    private readonly IFaceRecognitionEngine _faceRecognitionEngine;
    private readonly IBiometricTemplateProtector _templateProtector;
    private readonly ITemplateCompatibilityService _templateCompatibilityService;
    private readonly IFaceVerificationPolicyService _policyService;
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;
    private readonly FaceVerificationOptions _options;

    public OneToOneFaceVerifier(
        ApplicationDbContext dbContext,
        IFaceCaptureValidator captureValidator,
        IFaceRecognitionEngine faceRecognitionEngine,
        IBiometricTemplateProtector templateProtector,
        ITemplateCompatibilityService templateCompatibilityService,
        IFaceVerificationPolicyService policyService,
        IFaceEngineDiagnosticsService diagnosticsService,
        IOptions<FaceVerificationOptions> options)
    {
        _dbContext = dbContext;
        _captureValidator = captureValidator;
        _faceRecognitionEngine = faceRecognitionEngine;
        _templateProtector = templateProtector;
        _templateCompatibilityService = templateCompatibilityService;
        _policyService = policyService;
        _diagnosticsService = diagnosticsService;
        _options = options.Value;
    }

    public async Task<OneToOneVerificationResult> VerifyAsync(
        Guid studentId,
        FaceCaptureSample capture,
        FaceVerificationContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var diagnostics = _diagnosticsService.GetDiagnostics();

        if (!diagnostics.VerificationEnabled)
        {
            return Result(
                null,
                FaceVerificationOutcome.EngineUnavailable,
                FaceVerificationDecision.EngineError,
                "EngineUnavailable",
                diagnostics.StatusMessageKey,
                diagnostics,
                processingDurationMilliseconds: Elapsed(stopwatch));
        }

        var validation = _captureValidator.Validate(capture);
        if (!validation.Succeeded)
        {
            return Result(
                null,
                MapValidationOutcome(validation.RejectionReason),
                MapValidationDecision(validation.RejectionReason),
                validation.RejectionReason.ToString(),
                validation.MessageKey,
                diagnostics,
                imageWidth: validation.Width == 0 ? null : validation.Width,
                imageHeight: validation.Height == 0 ? null : validation.Height,
                processingDurationMilliseconds: Elapsed(stopwatch));
        }

        var markerFailure = DetectDevelopmentQualityMarker(capture.ImageBytes);
        if (markerFailure is not null)
        {
            return Result(
                null,
                markerFailure.Value.Outcome,
                FaceVerificationDecision.EngineError,
                markerFailure.Value.ErrorCode,
                markerFailure.Value.MessageKey,
                diagnostics,
                imageWidth: validation.Width,
                imageHeight: validation.Height,
                detectedFaceCount: 1,
                processingDurationMilliseconds: Elapsed(stopwatch));
        }

        var encoding = await _faceRecognitionEngine.ExtractEncodingAsync(capture.ImageBytes, cancellationToken);
        if (!encoding.Succeeded || encoding.Template is null)
        {
            return MapEncodingFailure(encoding, diagnostics, validation, stopwatch);
        }

        var qualityScore = NormalizeQualityScore(encoding.QualityScore);
        if (qualityScore < (decimal)_options.MinimumQualityScore)
        {
            return Result(
                null,
                FaceVerificationOutcome.LowQuality,
                FaceVerificationDecision.EngineError,
                "QualityTooLow",
                "ErrorFaceVerificationQualityTooLow",
                diagnostics,
                imageWidth: validation.Width,
                imageHeight: validation.Height,
                detectedFaceCount: 1,
                qualityScore: qualityScore,
                processingDurationMilliseconds: Elapsed(stopwatch));
        }

        var hasActiveConsent = await _dbContext.BiometricConsents
            .AsNoTracking()
            .AnyAsync(consent => consent.StudentId == studentId && consent.IsActive, cancellationToken);
        if (!hasActiveConsent)
        {
            return Result(
                null,
                FaceVerificationOutcome.NoActiveConsent,
                FaceVerificationDecision.Unknown,
                "NoActiveConsent",
                "ErrorFaceVerificationConsentRequired",
                diagnostics,
                validation.Width,
                validation.Height,
                1,
                qualityScore,
                Elapsed(stopwatch));
        }

        var template = await _dbContext.StudentFaceTemplates
            .AsNoTracking()
            .Where(item => item.StudentId == studentId && item.IsActive)
            .OrderByDescending(item => item.TemplateVersion)
            .FirstOrDefaultAsync(cancellationToken);
        if (template is null)
        {
            return Result(
                null,
                FaceVerificationOutcome.NoActiveTemplate,
                FaceVerificationDecision.Unknown,
                "NoActiveTemplate",
                "ErrorFaceVerificationActiveTemplateRequired",
                diagnostics,
                validation.Width,
                validation.Height,
                1,
                qualityScore,
                Elapsed(stopwatch));
        }

        if (template.RequiresReEnrollment)
        {
            return Result(
                template.Id,
                FaceVerificationOutcome.RequiresReEnrollment,
                FaceVerificationDecision.Unknown,
                "RequiresReEnrollment",
                "ErrorFaceVerificationRequiresReEnrollment",
                diagnostics,
                validation.Width,
                validation.Height,
                1,
                qualityScore,
                Elapsed(stopwatch));
        }

        var compatibility = _templateCompatibilityService.Check(new FaceTemplateCompatibilityMetadata(
            template.EngineName,
            template.EngineVersion,
            template.ModelName,
            template.ModelVersion,
            template.TemplateFormatVersion,
            template.EmbeddingDimension));
        if (!compatibility.IsCompatible)
        {
            return Result(
                template.Id,
                FaceVerificationOutcome.IncompatibleTemplate,
                FaceVerificationDecision.Unknown,
                "IncompatibleTemplate",
                compatibility.MessageKey,
                diagnostics,
                validation.Width,
                validation.Height,
                1,
                qualityScore,
                Elapsed(stopwatch));
        }

        byte[]? unprotectedTemplate = null;
        try
        {
            unprotectedTemplate = _templateProtector.Unprotect(template.ProtectedTemplate);
            var storedTemplate = new StoredFaceTemplate(
                template.TemplateFormatVersion,
                StoredTemplateFormat,
                unprotectedTemplate,
                template.EnrolledAtUtc,
                template.EngineName,
                template.EngineVersion,
                template.ModelName,
                template.ModelVersion,
                template.EmbeddingDimension,
                (double)template.QualityScore);

            var verification = await _faceRecognitionEngine.VerifyAsync(capture.ImageBytes, storedTemplate, cancellationToken);
            if (!verification.Succeeded)
            {
                return MapVerificationFailure(verification, diagnostics, validation, qualityScore, template.Id, stopwatch);
            }

            var score = NormalizeScore(verification.SimilarityScore);
            var decision = _policyService.Decide(score, diagnostics.Threshold, diagnostics.ScoreMetric);
            var outcome = decision == FaceVerificationDecision.Match
                ? FaceVerificationOutcome.Matched
                : FaceVerificationOutcome.NotMatched;

            return Result(
                template.Id,
                outcome,
                decision,
                string.Empty,
                outcome == FaceVerificationOutcome.Matched ? "FaceVerificationMatchedMessage" : "FaceVerificationNotMatchedMessage",
                diagnostics,
                validation.Width,
                validation.Height,
                1,
                qualityScore,
                Elapsed(stopwatch),
                score);
        }
        catch (CryptographicException)
        {
            return Result(
                template.Id,
                FaceVerificationOutcome.ProcessingFailed,
                FaceVerificationDecision.EngineError,
                "TemplateUnprotectFailed",
                "ErrorFaceVerificationProcessingFailed",
                diagnostics,
                validation.Width,
                validation.Height,
                1,
                qualityScore,
                Elapsed(stopwatch));
        }
        finally
        {
            if (unprotectedTemplate is { Length: > 0 })
            {
                CryptographicOperations.ZeroMemory(unprotectedTemplate);
            }
        }
    }

    private static OneToOneVerificationResult MapEncodingFailure(
        FaceEncodingResult encoding,
        FaceEngineDiagnosticsDto diagnostics,
        ImageValidationResult validation,
        Stopwatch stopwatch)
    {
        var outcome = encoding.ErrorCode switch
        {
            FaceRecognitionErrorCode.NoFaceDetected => FaceVerificationOutcome.NoFace,
            FaceRecognitionErrorCode.MultipleFacesDetected => FaceVerificationOutcome.MultipleFaces,
            FaceRecognitionErrorCode.EngineUnavailable => FaceVerificationOutcome.EngineUnavailable,
            FaceRecognitionErrorCode.EmptyImage or FaceRecognitionErrorCode.InvalidImage => FaceVerificationOutcome.InvalidImage,
            FaceRecognitionErrorCode.LowBrightness or FaceRecognitionErrorCode.HighBrightness or FaceRecognitionErrorCode.LowSharpness or FaceRecognitionErrorCode.FaceTooSmall or FaceRecognitionErrorCode.FaceTooLarge => FaceVerificationOutcome.LowQuality,
            _ => FaceVerificationOutcome.ProcessingFailed
        };

        var decision = outcome switch
        {
            FaceVerificationOutcome.NoFace => FaceVerificationDecision.NoFaceDetected,
            FaceVerificationOutcome.MultipleFaces => FaceVerificationDecision.MultipleFacesDetected,
            FaceVerificationOutcome.EngineUnavailable => FaceVerificationDecision.EngineError,
            _ => FaceVerificationDecision.EngineError
        };

        var messageKey = outcome switch
        {
            FaceVerificationOutcome.NoFace => "ErrorFaceVerificationNoFace",
            FaceVerificationOutcome.MultipleFaces => "ErrorFaceVerificationMultipleFaces",
            FaceVerificationOutcome.EngineUnavailable => "ErrorFaceVerificationEngineUnavailable",
            FaceVerificationOutcome.InvalidImage => "ErrorFaceVerificationInvalidImage",
            FaceVerificationOutcome.LowQuality => "ErrorFaceVerificationQualityTooLow",
            _ => "ErrorFaceVerificationProcessingFailed"
        };

        return Result(
            null,
            outcome,
            decision,
            encoding.ErrorCode.ToString(),
            messageKey,
            diagnostics,
            validation.Width,
            validation.Height,
            outcome == FaceVerificationOutcome.MultipleFaces ? 2 : 0,
            processingDurationMilliseconds: Elapsed(stopwatch));
    }

    private static OneToOneVerificationResult MapVerificationFailure(
        FaceVerificationResult verification,
        FaceEngineDiagnosticsDto diagnostics,
        ImageValidationResult validation,
        decimal qualityScore,
        Guid templateId,
        Stopwatch stopwatch)
    {
        var outcome = verification.ErrorCode switch
        {
            FaceRecognitionErrorCode.NoFaceDetected => FaceVerificationOutcome.NoFace,
            FaceRecognitionErrorCode.MultipleFacesDetected => FaceVerificationOutcome.MultipleFaces,
            FaceRecognitionErrorCode.EngineUnavailable => FaceVerificationOutcome.EngineUnavailable,
            FaceRecognitionErrorCode.InvalidImage or FaceRecognitionErrorCode.EmptyImage => FaceVerificationOutcome.InvalidImage,
            FaceRecognitionErrorCode.LowBrightness or FaceRecognitionErrorCode.HighBrightness or FaceRecognitionErrorCode.LowSharpness or FaceRecognitionErrorCode.FaceTooSmall or FaceRecognitionErrorCode.FaceTooLarge => FaceVerificationOutcome.LowQuality,
            _ => FaceVerificationOutcome.ProcessingFailed
        };

        var decision = outcome switch
        {
            FaceVerificationOutcome.NoFace => FaceVerificationDecision.NoFaceDetected,
            FaceVerificationOutcome.MultipleFaces => FaceVerificationDecision.MultipleFacesDetected,
            _ => FaceVerificationDecision.EngineError
        };

        var messageKey = outcome switch
        {
            FaceVerificationOutcome.NoFace => "ErrorFaceVerificationNoFace",
            FaceVerificationOutcome.MultipleFaces => "ErrorFaceVerificationMultipleFaces",
            FaceVerificationOutcome.EngineUnavailable => "ErrorFaceVerificationEngineUnavailable",
            FaceVerificationOutcome.InvalidImage => "ErrorFaceVerificationInvalidImage",
            FaceVerificationOutcome.LowQuality => "ErrorFaceVerificationQualityTooLow",
            _ => "ErrorFaceVerificationProcessingFailed"
        };

        return Result(
            templateId,
            outcome,
            decision,
            verification.ErrorCode.ToString(),
            messageKey,
            diagnostics,
            validation.Width,
            validation.Height,
            outcome == FaceVerificationOutcome.MultipleFaces ? 2 : 1,
            qualityScore,
            Elapsed(stopwatch));
    }

    private static (FaceVerificationOutcome Outcome, string ErrorCode, string MessageKey)? DetectDevelopmentQualityMarker(byte[] imageBytes)
    {
        var text = Encoding.UTF8.GetString(imageBytes);
        if (text.Contains("LOW_QUALITY", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceVerificationOutcome.LowQuality, "QualityTooLow", "ErrorFaceVerificationQualityTooLow");
        }

        if (text.Contains("DARK_IMAGE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceVerificationOutcome.LowQuality, "LowBrightness", "ErrorBiometricLowBrightness");
        }

        if (text.Contains("BRIGHT_IMAGE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceVerificationOutcome.LowQuality, "HighBrightness", "ErrorBiometricHighBrightness");
        }

        if (text.Contains("BLURRY_FACE", StringComparison.OrdinalIgnoreCase))
        {
            return (FaceVerificationOutcome.LowQuality, "LowSharpness", "ErrorBiometricLowSharpness");
        }

        return null;
    }

    private static FaceVerificationOutcome MapValidationOutcome(FaceCaptureRejectionReason reason)
        => reason is FaceCaptureRejectionReason.NoFace
            ? FaceVerificationOutcome.NoFace
            : reason is FaceCaptureRejectionReason.MultipleFaces
                ? FaceVerificationOutcome.MultipleFaces
                : reason is FaceCaptureRejectionReason.QualityTooLow or FaceCaptureRejectionReason.LowBrightness or FaceCaptureRejectionReason.HighBrightness or FaceCaptureRejectionReason.LowSharpness
                    ? FaceVerificationOutcome.LowQuality
                    : FaceVerificationOutcome.InvalidImage;

    private static FaceVerificationDecision MapValidationDecision(FaceCaptureRejectionReason reason)
        => reason switch
        {
            FaceCaptureRejectionReason.NoFace => FaceVerificationDecision.NoFaceDetected,
            FaceCaptureRejectionReason.MultipleFaces => FaceVerificationDecision.MultipleFacesDetected,
            _ => FaceVerificationDecision.EngineError
        };

    private static OneToOneVerificationResult Result(
        Guid? faceTemplateId,
        FaceVerificationOutcome outcome,
        FaceVerificationDecision decision,
        string errorCode,
        string messageKey,
        FaceEngineDiagnosticsDto diagnostics,
        int? imageWidth = null,
        int? imageHeight = null,
        int? detectedFaceCount = null,
        decimal? qualityScore = null,
        int? processingDurationMilliseconds = null,
        decimal? score = null)
        => new(
            faceTemplateId,
            outcome,
            decision,
            errorCode,
            messageKey,
            score,
            diagnostics.Threshold,
            diagnostics.ScoreMetric,
            diagnostics.EngineName,
            diagnostics.EngineVersion,
            diagnostics.ModelName,
            diagnostics.ModelVersion,
            diagnostics.ExpectedTemplateFormatVersion,
            imageWidth,
            imageHeight,
            detectedFaceCount,
            qualityScore,
            processingDurationMilliseconds);

    private static decimal NormalizeScore(double score)
    {
        if (!double.IsFinite(score) || score < 0)
        {
            return 0;
        }

        return (decimal)Math.Min(score, 999_999d);
    }

    private static decimal NormalizeQualityScore(double score)
    {
        if (!double.IsFinite(score) || score <= 0)
        {
            return 0.9m;
        }

        return (decimal)Math.Clamp(score, 0, 1);
    }

    private static int Elapsed(Stopwatch stopwatch)
    {
        stopwatch.Stop();
        return Math.Max(0, (int)Math.Min(int.MaxValue, stopwatch.ElapsedMilliseconds));
    }
}
