using AttendAI.Application.Common.Models;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Biometrics;

public interface IFaceCaptureValidator
{
    ImageValidationResult Validate(FaceCaptureSample sample);
}

public interface IFaceEnrollmentProcessor
{
    Task<FaceEnrollmentProcessingResult> ProcessAsync(
        IReadOnlyCollection<FaceCaptureSample> samples,
        CancellationToken cancellationToken = default);
}

public interface IBiometricTemplateProtector
{
    byte[] Protect(byte[] template);

    byte[] Unprotect(byte[] protectedTemplate);
}

public interface IBiometricEnrollmentRateLimiter
{
    bool TryAcquire(string userId, DateTimeOffset nowUtc);
}

public interface IFaceEngineReadinessService
{
    FaceEngineReadinessDto GetReadiness();
}

public interface IBiometricEnrollmentService
{
    Task<StudentBiometricStatusDto> GetStudentStatusAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FaceEnrollmentEventDto>> GetStudentEventsAsync(string userId, CancellationToken cancellationToken = default);

    Task<OperationResult> AcceptConsentAsync(string userId, CancellationToken cancellationToken = default);

    Task<OperationResult> EnrollAsync(string userId, FaceEnrollmentCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> WithdrawConsentAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResult<AdminBiometricEnrollmentRowDto>> GetAdminPagedAsync(AdminBiometricEnrollmentQuery query, CancellationToken cancellationToken = default);

    Task<AdminBiometricEnrollmentDetailsDto?> GetAdminDetailsAsync(Guid studentId, CancellationToken cancellationToken = default);

    Task<OperationResult> AdminRevokeAsync(Guid studentId, string performedByUserId, AdminBiometricActionCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> AdminRequireReEnrollmentAsync(Guid studentId, string performedByUserId, AdminBiometricActionCommand command, CancellationToken cancellationToken = default);
}
