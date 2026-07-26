namespace AttendAI.Application.Attendance;

public sealed class AttendanceOptions
{
    public const string SectionName = "Attendance";

    public bool AttendanceEnabled { get; set; } = true;

    public bool RequireRealFaceEngine { get; set; } = true;

    public bool RequireBrowserLocation { get; set; } = true;

    public bool ConsumeChallengeOnRejectedAttempt { get; set; } = true;

    public int ChallengeLifetimeMinutes { get; set; } = 5;

    public int MaximumAcceptedAccuracyMeters { get; set; } = 75;

    public int MaximumAttemptsPerWindow { get; set; } = 6;

    public int AttemptWindowMinutes { get; set; } = 10;

    public int CooldownSeconds { get; set; } = 5;

    public int IdempotencyKeyMaximumLength { get; set; } = 80;
}
