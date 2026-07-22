namespace AttendAI.Domain.Enums;

public enum FaceEnrollmentStatus
{
    NotEnrolled = 0,
    EnrollmentInProgress = 1,
    Active = 2,
    RequiresReEnrollment = 3,
    Revoked = 4,
    ConsentWithdrawn = 5,
    EngineUnavailable = 6
}
