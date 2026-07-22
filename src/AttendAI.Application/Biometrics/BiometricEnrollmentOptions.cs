namespace AttendAI.Application.Biometrics;

public sealed class BiometricEnrollmentOptions
{
    public const string SectionName = "BiometricEnrollment";

    public string ConsentVersion { get; set; } = "sprint4-v1";

    public string ConsentTextCanonical { get; set; } = "AttendAI biometric enrollment privacy notice sprint4-v1.";

    public int RequiredCaptureCount { get; set; } = 3;

    public int MaximumCaptureBytes { get; set; } = 1_500_000;

    public int MaximumEnrollmentRequestBytes { get; set; } = 6_000_000;

    public int MinimumImageWidth { get; set; } = 80;

    public int MinimumImageHeight { get; set; } = 80;

    public int MaximumImageWidth { get; set; } = 4096;

    public int MaximumImageHeight { get; set; } = 4096;

    public string[] AllowedMimeTypes { get; set; } = ["image/jpeg", "image/png"];

    public decimal MinimumQualityScore { get; set; } = 0.65m;

    public int EnrollmentRateLimitWindowMinutes { get; set; } = 15;

    public int EnrollmentRateLimitPermitCount { get; set; } = 5;
}
