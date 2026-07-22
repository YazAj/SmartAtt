using AttendAI.Application.FaceRecognition;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Biometrics;

public sealed record ImageValidationResult(
    bool Succeeded,
    FaceCaptureRejectionReason RejectionReason,
    string MessageKey,
    int Width,
    int Height,
    long ByteLength)
{
    public static ImageValidationResult Success(int width, int height, long byteLength)
        => new(true, FaceCaptureRejectionReason.None, string.Empty, width, height, byteLength);

    public static ImageValidationResult Failure(FaceCaptureRejectionReason reason, string messageKey)
        => new(false, reason, messageKey, 0, 0, 0);
}

public sealed record FaceEnrollmentProcessingResult(
    bool Succeeded,
    StoredFaceTemplate? Template,
    FaceCaptureRejectionReason RejectionReason,
    string MessageKey,
    int FaceCount,
    int CaptureCount,
    decimal QualityScore,
    decimal BrightnessScore,
    decimal SharpnessScore,
    decimal FaceAreaRatio,
    decimal PoseScore,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion)
{
    public static FaceEnrollmentProcessingResult Success(
        StoredFaceTemplate template,
        int captureCount,
        decimal qualityScore,
        decimal brightnessScore,
        decimal sharpnessScore,
        decimal faceAreaRatio,
        decimal poseScore)
        => new(
            true,
            template,
            FaceCaptureRejectionReason.None,
            "BiometricEnrollmentSucceeded",
            1,
            captureCount,
            qualityScore,
            brightnessScore,
            sharpnessScore,
            faceAreaRatio,
            poseScore,
            template.EngineName,
            template.EngineVersion,
            template.ModelName,
            template.ModelVersion);

    public static FaceEnrollmentProcessingResult Failure(
        FaceCaptureRejectionReason reason,
        string messageKey,
        int faceCount = 0,
        string engineName = "",
        string engineVersion = "")
        => new(false, null, reason, messageKey, faceCount, 0, 0, 0, 0, 0, 0, engineName, engineVersion, string.Empty, string.Empty);
}
