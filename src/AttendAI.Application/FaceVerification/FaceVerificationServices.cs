using AttendAI.Application.Common.Models;
using AttendAI.Application.Biometrics;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.FaceVerification;

public interface IFaceVerificationService
{
    Task<OperationResult<FaceVerificationResultDto>> VerifyCurrentStudentAsync(
        string userId,
        FaceVerificationCommand command,
        CancellationToken cancellationToken = default);
}

public interface IFaceVerificationQueryService
{
    Task<StudentFaceVerificationStatusDto> GetStudentStatusAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaceVerificationAttemptDto>> GetStudentAttemptsAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<AdminFaceVerificationAttemptRowDto>> GetAdminAttemptsAsync(AdminFaceVerificationAttemptQuery query, CancellationToken cancellationToken = default);

    Task<AdminFaceVerificationAttemptDetailsDto?> GetAdminAttemptDetailsAsync(Guid attemptId, CancellationToken cancellationToken = default);
}

public interface IFaceVerificationEligibilityService
{
    Task<FaceVerificationEligibilityDto> GetEligibilityAsync(string userId, CancellationToken cancellationToken = default);
}

public interface IFaceVerificationPolicyService
{
    bool IsMatch(decimal score, decimal threshold, ScoreMetric metric);

    FaceVerificationDecision Decide(decimal score, decimal threshold, ScoreMetric metric);
}

public interface IFaceVerificationRateLimiter
{
    bool TryAcquire(string userId, DateTimeOffset nowUtc);
}

public interface IFaceEngineDiagnosticsService
{
    FaceEngineDiagnosticsDto GetDiagnostics();
}

public interface ITemplateCompatibilityService
{
    TemplateCompatibilityResult Check(FaceTemplateCompatibilityMetadata metadata);
}

public interface IOneToOneFaceVerifier
{
    Task<OneToOneVerificationResult> VerifyAsync(
        Guid studentId,
        FaceCaptureSample capture,
        FaceVerificationContext context,
        CancellationToken cancellationToken = default);
}
