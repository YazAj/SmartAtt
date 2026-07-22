using AttendAI.Domain.Common;
using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class FaceVerificationAttempt : AuditableEntity
{
    private FaceVerificationAttempt()
    {
        ErrorCode = string.Empty;
        EngineName = string.Empty;
        EngineVersion = string.Empty;
        ModelName = string.Empty;
        ModelVersion = string.Empty;
        TemplateFormatVersion = string.Empty;
        ClientRequestId = string.Empty;
        SafeDescription = string.Empty;
    }

    public FaceVerificationAttempt(
        Guid studentId,
        Guid? faceTemplateId,
        FaceVerificationPurpose verificationPurpose,
        FaceVerificationOutcome outcome,
        FaceVerificationDecision decision,
        string? errorCode,
        decimal? score,
        decimal threshold,
        ScoreMetric scoreMetric,
        string engineName,
        string engineVersion,
        string modelName,
        string modelVersion,
        string templateFormatVersion,
        DateTimeOffset attemptedAtUtc,
        string? clientRequestId,
        string safeDescription,
        int? imageWidth = null,
        int? imageHeight = null,
        int? detectedFaceCount = null,
        decimal? qualityScore = null,
        int? processingDurationMilliseconds = null)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (!Enum.IsDefined(verificationPurpose))
        {
            throw new ArgumentOutOfRangeException(nameof(verificationPurpose), "Verification purpose is not valid.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), "Verification outcome is not valid.");
        }

        if (!Enum.IsDefined(decision))
        {
            throw new ArgumentOutOfRangeException(nameof(decision), "Verification decision is not valid.");
        }

        if (!Enum.IsDefined(scoreMetric))
        {
            throw new ArgumentOutOfRangeException(nameof(scoreMetric), "Score metric is not valid.");
        }

        if (score < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(score), "Score cannot be negative.");
        }

        if (threshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "Threshold cannot be negative.");
        }

        if (qualityScore is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(qualityScore), "Quality score must be between zero and one.");
        }

        if (imageWidth < 0 || imageHeight < 0 || detectedFaceCount < 0 || processingDurationMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(processingDurationMilliseconds), "Safe numeric metadata cannot be negative.");
        }

        StudentId = studentId;
        FaceTemplateId = faceTemplateId;
        VerificationPurpose = verificationPurpose;
        Outcome = outcome;
        Decision = decision;
        ErrorCode = OptionalText(errorCode, 80) ?? string.Empty;
        Score = score;
        Threshold = threshold;
        ScoreMetric = scoreMetric;
        EngineName = RequireText(engineName, nameof(engineName), 80);
        EngineVersion = RequireText(engineVersion, nameof(engineVersion), 80);
        ModelName = RequireText(modelName, nameof(modelName), 120);
        ModelVersion = RequireText(modelVersion, nameof(modelVersion), 80);
        TemplateFormatVersion = RequireText(templateFormatVersion, nameof(templateFormatVersion), 80);
        AttemptedAtUtc = attemptedAtUtc;
        ClientRequestId = OptionalText(clientRequestId, 80) ?? string.Empty;
        SafeDescription = RequireText(safeDescription, nameof(safeDescription), 300);
        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
        DetectedFaceCount = detectedFaceCount;
        QualityScore = qualityScore;
        ProcessingDurationMilliseconds = processingDurationMilliseconds;
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid? FaceTemplateId { get; private set; }

    public StudentFaceTemplate? FaceTemplate { get; private set; }

    public FaceVerificationPurpose VerificationPurpose { get; private set; }

    public FaceVerificationOutcome Outcome { get; private set; }

    public FaceVerificationDecision Decision { get; private set; }

    public string ErrorCode { get; private set; }

    public decimal? Score { get; private set; }

    public decimal Threshold { get; private set; }

    public ScoreMetric ScoreMetric { get; private set; }

    public string EngineName { get; private set; }

    public string EngineVersion { get; private set; }

    public string ModelName { get; private set; }

    public string ModelVersion { get; private set; }

    public string TemplateFormatVersion { get; private set; }

    public DateTimeOffset AttemptedAtUtc { get; private set; }

    public string ClientRequestId { get; private set; }

    public string SafeDescription { get; private set; }

    public int? ImageWidth { get; private set; }

    public int? ImageHeight { get; private set; }

    public int? DetectedFaceCount { get; private set; }

    public decimal? QualityScore { get; private set; }

    public int? ProcessingDurationMilliseconds { get; private set; }

    private static string RequireText(string value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{fieldName} cannot exceed {maxLength} characters.", fieldName);
        }

        return trimmed;
    }

    private static string? OptionalText(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", nameof(value));
        }

        return trimmed;
    }
}
