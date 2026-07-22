using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class FaceVerificationEligibilityService : IFaceVerificationEligibilityService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;
    private readonly ITemplateCompatibilityService _templateCompatibilityService;

    public FaceVerificationEligibilityService(
        ApplicationDbContext dbContext,
        IFaceEngineDiagnosticsService diagnosticsService,
        ITemplateCompatibilityService templateCompatibilityService)
    {
        _dbContext = dbContext;
        _diagnosticsService = diagnosticsService;
        _templateCompatibilityService = templateCompatibilityService;
    }

    public async Task<FaceVerificationEligibilityDto> GetEligibilityAsync(string userId, CancellationToken cancellationToken = default)
    {
        var diagnostics = _diagnosticsService.GetDiagnostics();
        var row = await (
            from student in _dbContext.Students.AsNoTracking()
            join user in _dbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.ApplicationUserId == userId
            select new StudentUserRow(
                student.Id,
                student.StudentNumber,
                student.NameEnglish,
                student.NameArabic,
                student.IsActive,
                user.IsActive,
                user.IsDisabled,
                user.MustChangePassword))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return new FaceVerificationEligibilityDto(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                diagnostics.VerificationEnabled,
                diagnostics.VerificationEnabled,
                false,
                FaceVerificationOutcome.Cancelled,
                "ErrorActiveStudentRequired",
                diagnostics.Mode,
                diagnostics.IsDemoMode);
        }

        var hasActiveConsent = await _dbContext.BiometricConsents
            .AsNoTracking()
            .AnyAsync(consent => consent.StudentId == row.StudentId && consent.IsActive, cancellationToken);
        var activeTemplate = await _dbContext.StudentFaceTemplates
            .AsNoTracking()
            .Where(template => template.StudentId == row.StudentId && template.IsActive)
            .OrderByDescending(template => template.TemplateVersion)
            .FirstOrDefaultAsync(cancellationToken);
        var latestTemplate = activeTemplate ?? await _dbContext.StudentFaceTemplates
            .AsNoTracking()
            .Where(template => template.StudentId == row.StudentId)
            .OrderByDescending(template => template.TemplateVersion)
            .FirstOrDefaultAsync(cancellationToken);

        var templateRequiresReEnrollment = latestTemplate?.RequiresReEnrollment == true;
        var templateCompatible = false;
        if (activeTemplate is not null)
        {
            templateCompatible = _templateCompatibilityService.Check(new FaceTemplateCompatibilityMetadata(
                activeTemplate.EngineName,
                activeTemplate.EngineVersion,
                activeTemplate.ModelName,
                activeTemplate.ModelVersion,
                activeTemplate.TemplateFormatVersion,
                activeTemplate.EmbeddingDimension)).IsCompatible;
        }

        var accountIsActive = row.UserIsActive && !row.UserIsDisabled;
        var passwordChangeComplete = !row.MustChangePassword;
        var engineAvailable = diagnostics.VerificationEnabled;
        var isEligible =
            accountIsActive &&
            row.StudentIsActive &&
            passwordChangeComplete &&
            hasActiveConsent &&
            activeTemplate is not null &&
            !templateRequiresReEnrollment &&
            templateCompatible &&
            diagnostics.VerificationEnabled;

        var (blockingOutcome, messageKey) = ResolveBlockingState(
            isEligible,
            accountIsActive,
            row.StudentIsActive,
            passwordChangeComplete,
            diagnostics.VerificationEnabled,
            hasActiveConsent,
            activeTemplate is not null,
            templateRequiresReEnrollment,
            templateCompatible,
            diagnostics.StatusMessageKey);

        return new FaceVerificationEligibilityDto(
            row.StudentId,
            row.StudentNumber,
            row.StudentNameEnglish,
            row.StudentNameArabic,
            true,
            accountIsActive,
            row.StudentIsActive,
            passwordChangeComplete,
            hasActiveConsent,
            activeTemplate is not null,
            templateRequiresReEnrollment,
            templateCompatible,
            diagnostics.VerificationEnabled,
            engineAvailable,
            isEligible,
            blockingOutcome,
            messageKey,
            diagnostics.Mode,
            diagnostics.IsDemoMode);
    }

    private static (FaceVerificationOutcome Outcome, string MessageKey) ResolveBlockingState(
        bool isEligible,
        bool accountIsActive,
        bool studentIsActive,
        bool passwordChangeComplete,
        bool verificationEnabled,
        bool hasActiveConsent,
        bool hasActiveTemplate,
        bool templateRequiresReEnrollment,
        bool templateCompatible,
        string engineMessageKey)
    {
        if (isEligible)
        {
            return (FaceVerificationOutcome.Cancelled, "FaceVerificationReadyMessage");
        }

        if (!accountIsActive || !studentIsActive)
        {
            return (FaceVerificationOutcome.Cancelled, "ErrorActiveStudentRequired");
        }

        if (!passwordChangeComplete)
        {
            return (FaceVerificationOutcome.Cancelled, "ErrorFaceVerificationPasswordChangeRequired");
        }

        if (!verificationEnabled)
        {
            return (FaceVerificationOutcome.EngineUnavailable, engineMessageKey);
        }

        if (!hasActiveConsent)
        {
            return (FaceVerificationOutcome.NoActiveConsent, "ErrorFaceVerificationConsentRequired");
        }

        if (templateRequiresReEnrollment)
        {
            return (FaceVerificationOutcome.RequiresReEnrollment, "ErrorFaceVerificationRequiresReEnrollment");
        }

        if (!hasActiveTemplate)
        {
            return (FaceVerificationOutcome.NoActiveTemplate, "ErrorFaceVerificationActiveTemplateRequired");
        }

        if (!templateCompatible)
        {
            return (FaceVerificationOutcome.IncompatibleTemplate, "ErrorFaceVerificationIncompatibleTemplate");
        }

        return (FaceVerificationOutcome.Cancelled, "ErrorFaceVerificationEligibility");
    }

    private sealed record StudentUserRow(
        Guid StudentId,
        string StudentNumber,
        string StudentNameEnglish,
        string StudentNameArabic,
        bool StudentIsActive,
        bool UserIsActive,
        bool UserIsDisabled,
        bool MustChangePassword);
}
