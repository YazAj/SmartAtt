namespace AttendAI.Application.FaceRecognition;

public sealed record StoredFaceTemplate(
    string Version,
    string Format,
    byte[] TemplateData,
    DateTimeOffset CreatedAtUtc);
