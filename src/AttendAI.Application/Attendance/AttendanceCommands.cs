using AttendAI.Application.Biometrics;

namespace AttendAI.Application.Attendance;

public sealed record AttendanceCheckInCommand(
    Guid LectureSessionId,
    string? ChallengeToken,
    string? IdempotencyKey,
    FaceCaptureSample Capture,
    BrowserLocationSample Location);

public sealed record BrowserLocationSample(
    double Latitude,
    double Longitude,
    double AccuracyMeters,
    DateTimeOffset? CapturedAtUtc);

public sealed record AttendanceLocationPolicy(
    decimal? Latitude,
    decimal? Longitude,
    int AllowedRadiusMeters,
    int MaximumAcceptedAccuracyMeters,
    bool LocationRequired);
