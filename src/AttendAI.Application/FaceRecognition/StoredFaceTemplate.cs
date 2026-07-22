namespace AttendAI.Application.FaceRecognition;

public sealed record StoredFaceTemplate(
    string Version,
    string Format,
    byte[] TemplateData,
    DateTimeOffset CreatedAtUtc,
    string EngineName = "Fake",
    string EngineVersion = "fake-sha256-v1",
    string ModelName = "Deterministic fake engine",
    string ModelVersion = "v1",
    int EncodingDimension = 0,
    double QualityScore = 0.9);
