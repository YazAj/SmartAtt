namespace AttendAI.Domain.Enums;

public enum LectureSessionEventType
{
    Started = 1,
    CodeGenerated = 2,
    CodeRegenerated = 3,
    Ended = 4,
    Cancelled = 5,
    Expired = 6,
    AdminForceEnded = 7
}
