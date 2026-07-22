using AttendAI.Application.Biometrics;
using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.Common.Models;
using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class FaceVerificationService : AcademicServiceBase, IFaceVerificationService
{
    private readonly IOneToOneFaceVerifier _oneToOneFaceVerifier;
    private readonly IFaceVerificationRateLimiter _rateLimiter;
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FaceVerificationService(
        ApplicationDbContext dbContext,
        IOneToOneFaceVerifier oneToOneFaceVerifier,
        IFaceVerificationRateLimiter rateLimiter,
        IFaceEngineDiagnosticsService diagnosticsService,
        IDateTimeProvider dateTimeProvider)
        : base(dbContext)
    {
        _oneToOneFaceVerifier = oneToOneFaceVerifier;
        _rateLimiter = rateLimiter;
        _diagnosticsService = diagnosticsService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<OperationResult<FaceVerificationResultDto>> VerifyCurrentStudentAsync(
        string userId,
        FaceVerificationCommand command,
        CancellationToken cancellationToken = default)
    {
        var student = await ResolveStudentAsync(userId, cancellationToken);
        if (student is null)
        {
            return OperationResult<FaceVerificationResultDto>.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var clientRequestId = NormalizeClientRequestId(command.ClientRequestId);
        if (!string.IsNullOrWhiteSpace(clientRequestId))
        {
            var existingAttempt = await DbContext.FaceVerificationAttempts
                .AsNoTracking()
                .Where(attempt => attempt.StudentId == student.StudentId && attempt.ClientRequestId == clientRequestId)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingAttempt is not null)
            {
                return OperationResult<FaceVerificationResultDto>.Success(ToResultDto(existingAttempt));
            }
        }

        if (!student.AccountIsActive || !student.StudentIsActive)
        {
            return await PersistDirectOutcomeAsync(
                student.StudentId,
                null,
                FaceVerificationOutcome.Cancelled,
                FaceVerificationDecision.Unknown,
                "InactiveStudent",
                "ErrorActiveStudentRequired",
                clientRequestId,
                cancellationToken);
        }

        if (student.MustChangePassword)
        {
            return await PersistDirectOutcomeAsync(
                student.StudentId,
                null,
                FaceVerificationOutcome.Cancelled,
                FaceVerificationDecision.Unknown,
                "PasswordChangeRequired",
                "ErrorFaceVerificationPasswordChangeRequired",
                clientRequestId,
                cancellationToken);
        }

        if (!_rateLimiter.TryAcquire(userId, _dateTimeProvider.UtcNow))
        {
            return await PersistDirectOutcomeAsync(
                student.StudentId,
                null,
                FaceVerificationOutcome.RateLimited,
                FaceVerificationDecision.Unknown,
                "RateLimited",
                "ErrorFaceVerificationRateLimit",
                clientRequestId,
                cancellationToken);
        }

        var verification = await _oneToOneFaceVerifier.VerifyAsync(
            student.StudentId,
            command.Capture,
            new FaceVerificationContext(FaceVerificationPurpose.SelfTest, userId, clientRequestId),
            cancellationToken);

        if (verification.Outcome == FaceVerificationOutcome.IncompatibleTemplate && verification.FaceTemplateId.HasValue)
        {
            await MarkTemplateForReEnrollmentAsync(verification.FaceTemplateId.Value, userId, cancellationToken);
        }

        var attempt = CreateAttempt(student.StudentId, verification, clientRequestId);
        DbContext.FaceVerificationAttempts.Add(attempt);
        var saveResult = await SaveChangesAsync(cancellationToken);
        if (!saveResult.Succeeded)
        {
            return OperationResult<FaceVerificationResultDto>.Failure(saveResult.Errors.ToArray());
        }

        return OperationResult<FaceVerificationResultDto>.Success(ToResultDto(attempt));
    }

    private async Task MarkTemplateForReEnrollmentAsync(
        Guid faceTemplateId,
        string userId,
        CancellationToken cancellationToken)
    {
        var template = await DbContext.StudentFaceTemplates
            .Where(item => item.Id == faceTemplateId && item.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        template?.Revoke(
            _dateTimeProvider.UtcNow,
            userId,
            "Template is incompatible with the configured face verification engine.",
            requiresReEnrollment: true);
    }

    private async Task<OperationResult<FaceVerificationResultDto>> PersistDirectOutcomeAsync(
        Guid studentId,
        Guid? faceTemplateId,
        FaceVerificationOutcome outcome,
        FaceVerificationDecision decision,
        string errorCode,
        string messageKey,
        string clientRequestId,
        CancellationToken cancellationToken)
    {
        var diagnostics = _diagnosticsService.GetDiagnostics();
        var verification = new OneToOneVerificationResult(
            faceTemplateId,
            outcome,
            decision,
            errorCode,
            messageKey,
            null,
            diagnostics.Threshold,
            diagnostics.ScoreMetric,
            diagnostics.EngineName,
            diagnostics.EngineVersion,
            diagnostics.ModelName,
            diagnostics.ModelVersion,
            diagnostics.ExpectedTemplateFormatVersion,
            null,
            null,
            null,
            null,
            null);

        var attempt = CreateAttempt(studentId, verification, clientRequestId);
        DbContext.FaceVerificationAttempts.Add(attempt);
        var saveResult = await SaveChangesAsync(cancellationToken);
        if (!saveResult.Succeeded)
        {
            return OperationResult<FaceVerificationResultDto>.Failure(saveResult.Errors.ToArray());
        }

        return OperationResult<FaceVerificationResultDto>.Success(ToResultDto(attempt));
    }

    private FaceVerificationAttempt CreateAttempt(
        Guid studentId,
        OneToOneVerificationResult verification,
        string clientRequestId)
        => new(
            studentId,
            verification.FaceTemplateId,
            FaceVerificationPurpose.SelfTest,
            verification.Outcome,
            verification.Decision,
            verification.ErrorCode,
            verification.Score,
            verification.Threshold,
            verification.ScoreMetric,
            verification.EngineName,
            verification.EngineVersion,
            verification.ModelName,
            verification.ModelVersion,
            verification.TemplateFormatVersion,
            _dateTimeProvider.UtcNow,
            clientRequestId,
            SafeDescriptionKey(verification.Outcome),
            verification.ImageWidth,
            verification.ImageHeight,
            verification.DetectedFaceCount,
            verification.QualityScore,
            verification.ProcessingDurationMilliseconds);

    private FaceVerificationResultDto ToResultDto(FaceVerificationAttempt attempt)
    {
        var diagnostics = _diagnosticsService.GetDiagnostics();
        return new FaceVerificationResultDto(
            attempt.Id,
            attempt.Outcome,
            attempt.Decision,
            attempt.Decision == FaceVerificationDecision.Match,
            MessageKey(attempt.Outcome),
            attempt.Score,
            attempt.Threshold,
            attempt.ScoreMetric,
            attempt.EngineName,
            attempt.EngineVersion,
            attempt.ModelName,
            attempt.ModelVersion,
            attempt.TemplateFormatVersion,
            diagnostics.IsDemoMode,
            attempt.ImageWidth,
            attempt.ImageHeight,
            attempt.DetectedFaceCount,
            attempt.QualityScore,
            attempt.ProcessingDurationMilliseconds);
    }

    private async Task<StudentUserRow?> ResolveStudentAsync(string userId, CancellationToken cancellationToken)
        => await (
            from student in DbContext.Students.AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.ApplicationUserId == userId
            select new StudentUserRow(
                student.Id,
                student.IsActive,
                user.IsActive && !user.IsDisabled,
                user.MustChangePassword))
            .FirstOrDefaultAsync(cancellationToken);

    private static string NormalizeClientRequestId(string? clientRequestId)
    {
        var normalized = clientRequestId?.Trim() ?? string.Empty;
        return normalized.Length <= 80 ? normalized : normalized[..80];
    }

    private static string SafeDescriptionKey(FaceVerificationOutcome outcome)
        => outcome switch
        {
            FaceVerificationOutcome.Matched => "FaceVerificationAttemptDescriptionMatched",
            FaceVerificationOutcome.NotMatched => "FaceVerificationAttemptDescriptionNotMatched",
            FaceVerificationOutcome.NoActiveConsent => "FaceVerificationAttemptDescriptionNoActiveConsent",
            FaceVerificationOutcome.NoActiveTemplate => "FaceVerificationAttemptDescriptionNoActiveTemplate",
            FaceVerificationOutcome.RequiresReEnrollment => "FaceVerificationAttemptDescriptionRequiresReEnrollment",
            FaceVerificationOutcome.IncompatibleTemplate => "FaceVerificationAttemptDescriptionIncompatibleTemplate",
            FaceVerificationOutcome.NoFace => "FaceVerificationAttemptDescriptionNoFace",
            FaceVerificationOutcome.MultipleFaces => "FaceVerificationAttemptDescriptionMultipleFaces",
            FaceVerificationOutcome.LowQuality => "FaceVerificationAttemptDescriptionLowQuality",
            FaceVerificationOutcome.InvalidImage => "FaceVerificationAttemptDescriptionInvalidImage",
            FaceVerificationOutcome.RateLimited => "FaceVerificationAttemptDescriptionRateLimited",
            FaceVerificationOutcome.EngineUnavailable => "FaceVerificationAttemptDescriptionEngineUnavailable",
            FaceVerificationOutcome.ProcessingFailed => "FaceVerificationAttemptDescriptionProcessingFailed",
            _ => "FaceVerificationAttemptDescriptionCancelled"
        };

    private static string MessageKey(FaceVerificationOutcome outcome)
        => outcome switch
        {
            FaceVerificationOutcome.Matched => "FaceVerificationMatchedMessage",
            FaceVerificationOutcome.NotMatched => "FaceVerificationNotMatchedMessage",
            FaceVerificationOutcome.NoActiveConsent => "ErrorFaceVerificationConsentRequired",
            FaceVerificationOutcome.NoActiveTemplate => "ErrorFaceVerificationActiveTemplateRequired",
            FaceVerificationOutcome.RequiresReEnrollment => "ErrorFaceVerificationRequiresReEnrollment",
            FaceVerificationOutcome.IncompatibleTemplate => "ErrorFaceVerificationIncompatibleTemplate",
            FaceVerificationOutcome.NoFace => "ErrorFaceVerificationNoFace",
            FaceVerificationOutcome.MultipleFaces => "ErrorFaceVerificationMultipleFaces",
            FaceVerificationOutcome.LowQuality => "ErrorFaceVerificationQualityTooLow",
            FaceVerificationOutcome.InvalidImage => "ErrorFaceVerificationInvalidImage",
            FaceVerificationOutcome.RateLimited => "ErrorFaceVerificationRateLimit",
            FaceVerificationOutcome.EngineUnavailable => "ErrorFaceVerificationEngineUnavailable",
            FaceVerificationOutcome.ProcessingFailed => "ErrorFaceVerificationProcessingFailed",
            _ => "ErrorFaceVerificationEligibility"
        };

    private sealed record StudentUserRow(
        Guid StudentId,
        bool StudentIsActive,
        bool AccountIsActive,
        bool MustChangePassword);
}
