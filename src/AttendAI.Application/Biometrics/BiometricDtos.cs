using AttendAI.Domain.Enums;

namespace AttendAI.Application.Biometrics;

public sealed record StudentBiometricStatusDto(
    Guid? StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    bool StudentIsActive,
    FaceEnrollmentStatus Status,
    bool HasActiveConsent,
    bool HasActiveTemplate,
    string? ConsentVersion,
    DateTimeOffset? ConsentAcceptedAtUtc,
    DateTimeOffset? ConsentWithdrawnAtUtc,
    DateTimeOffset? EnrolledAtUtc,
    DateTimeOffset? RevokedAtUtc,
    string EngineMode,
    bool EngineAllowsEnrollment,
    string EngineStatusMessageKey);

public sealed record FaceTemplateMetadataDto(
    Guid Id,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    string TemplateFormatVersion,
    int EmbeddingDimension,
    decimal QualityScore,
    int CaptureCount,
    int TemplateVersion,
    DateTimeOffset EnrolledAtUtc,
    DateTimeOffset? RevokedAtUtc,
    string? RevocationReason,
    bool IsActive,
    bool RequiresReEnrollment,
    string RowVersion);

public sealed record FaceEnrollmentEventDto(
    DateTimeOffset OccurredAtUtc,
    FaceEnrollmentEventType EventType,
    string Outcome,
    string ErrorCode,
    string PerformedByUserId,
    string EngineName,
    string EngineVersion,
    string SafeDescription);

public sealed record AdminBiometricEnrollmentRowDto(
    Guid StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    string Email,
    string DepartmentNameEnglish,
    string DepartmentNameArabic,
    bool StudentIsActive,
    FaceEnrollmentStatus Status,
    DateTimeOffset? EnrolledAtUtc,
    DateTimeOffset? RevokedAtUtc,
    string? EngineName,
    int? TemplateVersion);

public sealed record AdminBiometricEnrollmentDetailsDto(
    Guid StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    string Email,
    string DepartmentNameEnglish,
    string DepartmentNameArabic,
    bool StudentIsActive,
    StudentBiometricStatusDto Status,
    FaceTemplateMetadataDto? ActiveTemplate,
    IReadOnlyList<FaceTemplateMetadataDto> TemplateHistory,
    IReadOnlyList<FaceEnrollmentEventDto> Events);

public sealed record FaceEngineReadinessDto(
    string Mode,
    string EngineName,
    string EngineVersion,
    string ModelName,
    string ModelVersion,
    bool EnrollmentEnabled,
    bool IsProductionSafe,
    string StatusMessageKey,
    string GateResult);
