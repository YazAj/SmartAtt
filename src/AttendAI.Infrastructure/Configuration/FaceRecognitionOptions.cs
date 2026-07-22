namespace AttendAI.Infrastructure.Configuration;

public sealed class FaceRecognitionOptions
{
    public const string SectionName = "FaceRecognition";

    public string Provider { get; set; } = "Fake";

    public string ModelPath { get; set; } = string.Empty;

    public double Threshold { get; set; } = 0.95;
}
