using AttendAI.Domain.Enums;

namespace AttendAI.Application.FaceVerification;

public sealed class FaceVerificationOptions
{
    public const string SectionName = "FaceVerification";

    public bool FaceVerificationEnabled { get; set; } = true;

    public bool RequireRealEngineForVerification { get; set; }

    public bool AllowFakeEngineInDevelopment { get; set; } = true;

    public double VerificationThreshold { get; set; } = 0.95;

    public ScoreMetric ScoreMetric { get; set; } = ScoreMetric.CosineSimilarity;

    public int MaximumCaptureBytes { get; set; } = 1_500_000;

    public int MaximumVerificationRequestBytes { get; set; } = 2_000_000;

    public int MinimumImageWidth { get; set; } = 80;

    public int MinimumImageHeight { get; set; } = 80;

    public int MaximumImageWidth { get; set; } = 4096;

    public int MaximumImageHeight { get; set; } = 4096;

    public string[] AllowedMimeTypes { get; set; } = ["image/jpeg", "image/png"];

    public double MinimumQualityScore { get; set; } = 0.65;

    public int MaximumAttemptsPerWindow { get; set; } = 8;

    public int AttemptWindowMinutes { get; set; } = 15;

    public int CooldownSeconds { get; set; } = 2;

    public int ExpectedEmbeddingDimension { get; set; } = 32;

    public string ExpectedTemplateFormatVersion { get; set; } = "fake-sha256-v1";
}
