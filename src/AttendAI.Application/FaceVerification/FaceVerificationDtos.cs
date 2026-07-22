using AttendAI.Domain.Enums;

namespace AttendAI.Application.FaceVerification;

public sealed record FaceVerificationEligibilityDto(
    Guid? StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    bool HasStudentProfile,
    bool AccountIsActive,
    bool StudentIsActive,
    bool PasswordChangeComplete,
    bool HasActiveConsent,
    bool HasActiveTemplate,
    bool TemplateRequiresReEnrollment,
    bool TemplateCompatible,
    bool VerificationEnabled,
    bool EngineAvailable,
    bool IsEligible,
    FaceVerificationOutcome BlockingOutcome,
    string MessageKey,
    string EngineMode,
    bool IsDemoMode);

public sealed record StudentFaceVerificationStatusDto(
    FaceVerificationEligibilityDto Eligibility,
    IReadOnlyList<FaceVerificationAttemptDto> RecentAttempts,
    FaceEngineDiagnosticsDto Diagnostics);

public sealed record FaceVerificationAttemptDto(
    Guid Id,
    Guid StudentId,
    Guid? FaceTemplateId,
    FaceVerificationPurpose VerificationPurpose,
    FaceVerificationOutcome Outcome,
    FaceVerificationDecision Decision,
    string ErrorCode,
    decimal? Score,
    decimal Threshold,
    ScoreMetric ScoreMetric,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    string TemplateFormatVersion,
    DateTimeOffset AttemptedAtUtc,
    string ClientRequestId,
    string SafeDescription,
    int? ImageWidth,
    int? ImageHeight,
    int? DetectedFaceCount,
    decimal? QualityScore,
    int? ProcessingDurationMilliseconds);

public sealed record FaceVerificationResultDto(
    Guid AttemptId,
    FaceVerificationOutcome Outcome,
    FaceVerificationDecision Decision,
    bool IsMatch,
    string MessageKey,
    decimal? Score,
    decimal Threshold,
    ScoreMetric ScoreMetric,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    string TemplateFormatVersion,
    bool IsDemoMode,
    int? ImageWidth,
    int? ImageHeight,
    int? DetectedFaceCount,
    decimal? QualityScore,
    int? ProcessingDurationMilliseconds);

public sealed record AdminFaceVerificationAttemptRowDto(
    Guid Id,
    Guid StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    string Email,
    string DepartmentNameEnglish,
    string DepartmentNameArabic,
    FaceVerificationPurpose VerificationPurpose,
    FaceVerificationOutcome Outcome,
    FaceVerificationDecision Decision,
    decimal? Score,
    decimal Threshold,
    ScoreMetric ScoreMetric,
    string EngineName,
    string ModelName,
    DateTimeOffset AttemptedAtUtc,
    int? ProcessingDurationMilliseconds);

public sealed record AdminFaceVerificationAttemptDetailsDto(
    AdminFaceVerificationAttemptRowDto Attempt,
    FaceVerificationAttemptDto Metadata);

public sealed record FaceEngineDiagnosticsDto(
    string Mode,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    bool VerificationEnabled,
    bool IsDemoMode,
    bool IsProductionSafe,
    string StatusMessageKey,
    string GateResult,
    decimal Threshold,
    ScoreMetric ScoreMetric,
    int MaximumAttemptsPerWindow,
    int AttemptWindowMinutes,
    int CooldownSeconds,
    string ExpectedTemplateFormatVersion,
    int ExpectedEmbeddingDimension);

public sealed record TemplateCompatibilityResult(
    bool IsCompatible,
    string MessageKey,
    string ExpectedEngineName,
    string ExpectedEngineVersion,
    string ExpectedModelName,
    string ExpectedModelVersion,
    string ExpectedTemplateFormatVersion,
    int ExpectedEmbeddingDimension);
