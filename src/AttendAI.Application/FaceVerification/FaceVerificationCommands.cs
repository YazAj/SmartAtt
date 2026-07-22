using AttendAI.Application.Biometrics;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.FaceVerification;

public sealed record FaceVerificationCommand(
    FaceCaptureSample Capture,
    string? ClientRequestId);

public sealed record FaceVerificationContext(
    FaceVerificationPurpose Purpose,
    string PerformedByUserId,
    string? ClientRequestId);

public sealed record FaceTemplateCompatibilityMetadata(
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    string TemplateFormatVersion,
    int EmbeddingDimension);
