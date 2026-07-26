using AttendAI.Domain.Common;
using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class AttendanceAttempt : AuditableEntity
{
    private AttendanceAttempt()
    {
        IdempotencyKeyHash = string.Empty;
        ChallengeTokenHash = string.Empty;
        SafeDescription = string.Empty;
    }

    public AttendanceAttempt(
        Guid studentId,
        Guid lectureSessionId,
        AttendanceAttemptOutcome outcome,
        AttendanceFailureReason failureReason,
        LocationVerificationOutcome locationOutcome,
        DateTimeOffset attemptedAtUtc,
        string idempotencyKeyHash,
        string? challengeTokenHash,
        string safeDescription,
        decimal? distanceMeters = null,
        decimal? browserAccuracyMeters = null,
        int? allowedRadiusMeters = null,
        int? maximumAcceptedAccuracyMeters = null,
        int? processingDurationMilliseconds = null)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (lectureSessionId == Guid.Empty)
        {
            throw new ArgumentException("Lecture session is required.", nameof(lectureSessionId));
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(nameof(outcome), "Attendance attempt outcome is not valid.");
        }

        if (!Enum.IsDefined(failureReason))
        {
            throw new ArgumentOutOfRangeException(nameof(failureReason), "Attendance failure reason is not valid.");
        }

        if (!Enum.IsDefined(locationOutcome))
        {
            throw new ArgumentOutOfRangeException(nameof(locationOutcome), "Location outcome is not valid.");
        }

        if (outcome == AttendanceAttemptOutcome.Succeeded && failureReason != AttendanceFailureReason.None)
        {
            throw new ArgumentException("Successful attempts cannot include a failure reason.", nameof(failureReason));
        }

        if (distanceMeters < 0 || browserAccuracyMeters < 0 || allowedRadiusMeters < 0 ||
            maximumAcceptedAccuracyMeters < 0 || processingDurationMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceMeters), "Safe numeric metadata cannot be negative.");
        }

        StudentId = studentId;
        LectureSessionId = lectureSessionId;
        Outcome = outcome;
        FailureReason = failureReason;
        LocationOutcome = locationOutcome;
        AttemptedAtUtc = attemptedAtUtc;
        IdempotencyKeyHash = RequireText(idempotencyKeyHash, nameof(idempotencyKeyHash), 128);
        ChallengeTokenHash = OptionalText(challengeTokenHash, 128) ?? string.Empty;
        SafeDescription = RequireText(safeDescription, nameof(safeDescription), 300);
        DistanceMeters = distanceMeters;
        BrowserAccuracyMeters = browserAccuracyMeters;
        AllowedRadiusMeters = allowedRadiusMeters;
        MaximumAcceptedAccuracyMeters = maximumAcceptedAccuracyMeters;
        ProcessingDurationMilliseconds = processingDurationMilliseconds;
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid LectureSessionId { get; private set; }

    public LectureSession? LectureSession { get; private set; }

    public Guid? FaceVerificationAttemptId { get; private set; }

    public FaceVerificationAttempt? FaceVerificationAttempt { get; private set; }

    public AttendanceAttemptOutcome Outcome { get; private set; }

    public AttendanceFailureReason FailureReason { get; private set; }

    public LocationVerificationOutcome LocationOutcome { get; private set; }

    public DateTimeOffset AttemptedAtUtc { get; private set; }

    public string IdempotencyKeyHash { get; private set; }

    public string ChallengeTokenHash { get; private set; }

    public string SafeDescription { get; private set; }

    public decimal? DistanceMeters { get; private set; }

    public decimal? BrowserAccuracyMeters { get; private set; }

    public int? AllowedRadiusMeters { get; private set; }

    public int? MaximumAcceptedAccuracyMeters { get; private set; }

    public int? ProcessingDurationMilliseconds { get; private set; }

    public void AttachFaceVerificationAttempt(Guid faceVerificationAttemptId)
    {
        if (faceVerificationAttemptId == Guid.Empty)
        {
            throw new ArgumentException("Face verification attempt is required.", nameof(faceVerificationAttemptId));
        }

        FaceVerificationAttemptId = faceVerificationAttemptId;
        UpdatedAtUtc = DateTimeOffset.UtcNow;
    }

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
