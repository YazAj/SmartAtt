using AttendAI.Domain.Common;
using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class FaceEnrollmentEvent : AuditableEntity
{
    private FaceEnrollmentEvent()
    {
        Outcome = string.Empty;
        ErrorCode = string.Empty;
        PerformedByUserId = string.Empty;
        EngineName = string.Empty;
        EngineVersion = string.Empty;
        SafeDescription = string.Empty;
    }

    public FaceEnrollmentEvent(
        Guid studentId,
        Guid? faceTemplateId,
        Guid? biometricConsentId,
        FaceEnrollmentEventType eventType,
        string outcome,
        string? errorCode,
        DateTimeOffset occurredAtUtc,
        string performedByUserId,
        string engineName,
        string engineVersion,
        string safeDescription)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (!Enum.IsDefined(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType), "Event type is not valid.");
        }

        StudentId = studentId;
        FaceTemplateId = faceTemplateId;
        BiometricConsentId = biometricConsentId;
        EventType = eventType;
        Outcome = (outcome ?? string.Empty).Trim();
        ErrorCode = (errorCode ?? string.Empty).Trim();
        OccurredAtUtc = occurredAtUtc;
        PerformedByUserId = (performedByUserId ?? string.Empty).Trim();
        EngineName = (engineName ?? string.Empty).Trim();
        EngineVersion = (engineVersion ?? string.Empty).Trim();
        SafeDescription = (safeDescription ?? string.Empty).Trim();
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid? FaceTemplateId { get; private set; }

    public StudentFaceTemplate? FaceTemplate { get; private set; }

    public Guid? BiometricConsentId { get; private set; }

    public BiometricConsent? BiometricConsent { get; private set; }

    public FaceEnrollmentEventType EventType { get; private set; }

    public string Outcome { get; private set; }

    public string ErrorCode { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string PerformedByUserId { get; private set; }

    public string EngineName { get; private set; }

    public string EngineVersion { get; private set; }

    public string SafeDescription { get; private set; }
}
