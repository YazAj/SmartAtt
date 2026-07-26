namespace AttendAI.Domain.Enums;

public enum AttendanceFailureReason
{
    None = 0,
    StudentNotFound = 1,
    StudentInactive = 2,
    PasswordChangeRequired = 3,
    NotEnrolled = 4,
    SessionNotFound = 5,
    SessionNotActive = 6,
    AttendanceWindowClosed = 7,
    AttendanceLocationUnavailable = 8,
    LocationInvalid = 9,
    LocationInaccurate = 10,
    OutsideGeofence = 11,
    ChallengeMissing = 12,
    ChallengeInvalid = 13,
    ChallengeExpired = 14,
    ChallengeAlreadyUsed = 15,
    IdempotencyKeyInvalid = 16,
    FaceVerificationFailed = 17,
    FaceNotMatched = 18,
    FaceNoFace = 19,
    FaceMultipleFaces = 20,
    FaceLowQuality = 21,
    FaceEngineUnavailable = 22,
    DuplicateAttendance = 23,
    PersistenceFailed = 24,
    RateLimited = 25
}
