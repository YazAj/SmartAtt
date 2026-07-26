namespace AttendAI.Domain.Enums;

public enum LocationVerificationOutcome
{
    NotEvaluated = 0,
    Accepted = 1,
    MissingPolicy = 2,
    Invalid = 3,
    Inaccurate = 4,
    OutsideGeofence = 5
}
