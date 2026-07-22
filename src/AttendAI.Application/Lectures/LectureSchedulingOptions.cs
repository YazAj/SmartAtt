namespace AttendAI.Application.Lectures;

public sealed class LectureSchedulingOptions
{
    public const string SectionName = "LectureScheduling";

    public string TimeZoneId { get; set; } = "Asia/Amman";

    public string WindowsTimeZoneId { get; set; } = "Jordan Standard Time";

    public int MinimumLectureDurationMinutes { get; set; } = 15;

    public int MaximumLectureDurationMinutes { get; set; } = 240;

    public int MinimumAllowedRadiusMeters { get; set; } = 0;

    public int MaximumAllowedRadiusMeters { get; set; } = 500;

    public int EarlyStartWindowMinutes { get; set; } = 15;

    public int LateStartWindowMinutes { get; set; } = 30;

    public int MaximumSessionDurationMinutes { get; set; } = 240;

    public int SessionCodeLength { get; set; } = 6;

    public int SessionCodeLifetimeMinutes { get; set; } = 15;

    public int SessionPollingSeconds { get; set; } = 20;
}
