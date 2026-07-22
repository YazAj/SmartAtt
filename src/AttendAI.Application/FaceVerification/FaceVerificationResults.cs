using AttendAI.Domain.Enums;

namespace AttendAI.Application.FaceVerification;

public sealed record OneToOneVerificationResult(
    Guid? FaceTemplateId,
    FaceVerificationOutcome Outcome,
    FaceVerificationDecision Decision,
    string ErrorCode,
    string MessageKey,
    decimal? Score,
    decimal Threshold,
    ScoreMetric ScoreMetric,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    string TemplateFormatVersion,
    int? ImageWidth,
    int? ImageHeight,
    int? DetectedFaceCount,
    decimal? QualityScore,
    int? ProcessingDurationMilliseconds);
