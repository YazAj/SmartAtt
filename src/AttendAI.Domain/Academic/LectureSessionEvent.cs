using AttendAI.Domain.Common;
using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class LectureSessionEvent : AuditableEntity
{
    private LectureSessionEvent()
    {
        PerformedByUserId = string.Empty;
        SafeDescription = string.Empty;
    }

    public LectureSessionEvent(
        Guid lectureSessionId,
        LectureSessionEventType eventType,
        DateTimeOffset occurredAtUtc,
        string performedByUserId,
        LectureSessionStatus? previousStatus,
        LectureSessionStatus? newStatus,
        string safeDescription)
    {
        if (lectureSessionId == Guid.Empty)
        {
            throw new ArgumentException("Lecture session is required.", nameof(lectureSessionId));
        }

        if (!Enum.IsDefined(eventType))
        {
            throw new ArgumentOutOfRangeException(nameof(eventType), "Event type is not valid.");
        }

        LectureSessionId = lectureSessionId;
        EventType = eventType;
        OccurredAtUtc = occurredAtUtc;
        PerformedByUserId = (performedByUserId ?? string.Empty).Trim();
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        SafeDescription = (safeDescription ?? string.Empty).Trim();
    }

    public Guid LectureSessionId { get; private set; }

    public LectureSession? LectureSession { get; private set; }

    public LectureSessionEventType EventType { get; private set; }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string PerformedByUserId { get; private set; }

    public LectureSessionStatus? PreviousStatus { get; private set; }

    public LectureSessionStatus? NewStatus { get; private set; }

    public string SafeDescription { get; private set; }
}
