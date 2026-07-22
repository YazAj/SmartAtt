namespace AttendAI.Domain.Enums;

public enum FaceVerificationOutcome
{
    Matched = 1,
    NotMatched = 2,
    NoActiveConsent = 3,
    NoActiveTemplate = 4,
    RequiresReEnrollment = 5,
    IncompatibleTemplate = 6,
    NoFace = 7,
    MultipleFaces = 8,
    LowQuality = 9,
    InvalidImage = 10,
    RateLimited = 11,
    EngineUnavailable = 12,
    ProcessingFailed = 13,
    Cancelled = 14
}
