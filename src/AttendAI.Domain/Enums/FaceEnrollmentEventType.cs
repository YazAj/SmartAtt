namespace AttendAI.Domain.Enums;

public enum FaceEnrollmentEventType
{
    ConsentAccepted = 0,
    ConsentWithdrawn = 1,
    EnrollmentStarted = 2,
    CaptureRejected = 3,
    TemplateCreated = 4,
    ReEnrollmentStarted = 5,
    TemplateReplaced = 6,
    TemplateRevoked = 7,
    AdminRevoked = 8,
    EnrollmentFailed = 9,
    EngineUnavailable = 10
}
