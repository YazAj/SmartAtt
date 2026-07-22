namespace AttendAI.Application.Biometrics;

public sealed record FaceCaptureSample(
    byte[] ImageBytes,
    string ContentType,
    string FieldName);

public sealed record FaceEnrollmentCommand(IReadOnlyCollection<FaceCaptureSample> Samples);

public sealed record AdminBiometricActionCommand(
    string Reason,
    string? RowVersion);
