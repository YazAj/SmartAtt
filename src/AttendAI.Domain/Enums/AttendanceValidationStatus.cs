namespace AttendAI.Domain.Enums;

public enum AttendanceValidationStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Duplicate = 3,
    OutsideLectureTime = 4,
    OutsideAllowedLocation = 5,
    FaceMismatch = 6
}
