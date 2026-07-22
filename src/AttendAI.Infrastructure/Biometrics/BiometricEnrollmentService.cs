using System.Data;
using System.Security.Cryptography;
using System.Text;
using AttendAI.Application.Biometrics;
using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Biometrics;

public sealed class BiometricEnrollmentService : AcademicServiceBase, IBiometricEnrollmentService
{
    private readonly IFaceEnrollmentProcessor _processor;
    private readonly IBiometricTemplateProtector _templateProtector;
    private readonly IBiometricEnrollmentRateLimiter _rateLimiter;
    private readonly IFaceEngineReadinessService _readinessService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly BiometricEnrollmentOptions _options;

    public BiometricEnrollmentService(
        ApplicationDbContext dbContext,
        IFaceEnrollmentProcessor processor,
        IBiometricTemplateProtector templateProtector,
        IBiometricEnrollmentRateLimiter rateLimiter,
        IFaceEngineReadinessService readinessService,
        IDateTimeProvider dateTimeProvider,
        IOptions<BiometricEnrollmentOptions> options)
        : base(dbContext)
    {
        _processor = processor;
        _templateProtector = templateProtector;
        _rateLimiter = rateLimiter;
        _readinessService = readinessService;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
    }

    public async Task<StudentBiometricStatusDto> GetStudentStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var student = await GetStudentRowByUserIdAsync(userId, tracking: false, cancellationToken);
        if (student is null)
        {
            var readiness = _readinessService.GetReadiness();
            return new StudentBiometricStatusDto(
                null,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                FaceEnrollmentStatus.NotEnrolled,
                false,
                false,
                null,
                null,
                null,
                null,
                null,
                readiness.Mode,
                readiness.EnrollmentEnabled,
                readiness.StatusMessageKey);
        }

        return await BuildStudentStatusAsync(student.Student, student.User, cancellationToken);
    }

    public async Task<IReadOnlyList<FaceEnrollmentEventDto>> GetStudentEventsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var studentId = await DbContext.Students
            .AsNoTracking()
            .Where(student => student.ApplicationUserId == userId)
            .Select(student => student.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return studentId == Guid.Empty
            ? []
            : await GetEventDtosAsync(studentId, cancellationToken);
    }

    public async Task<OperationResult> AcceptConsentAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var student = await DbContext.Students.FirstOrDefaultAsync(item => item.ApplicationUserId == userId && item.IsActive, cancellationToken);
        if (student is null)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var activeConsent = await DbContext.BiometricConsents.FirstOrDefaultAsync(
            item => item.StudentId == student.Id && item.IsActive,
            cancellationToken);

        if (activeConsent is not null)
        {
            return OperationResult.Success();
        }

        var consent = new BiometricConsent(
            student.Id,
            _options.ConsentVersion,
            ConsentHash(),
            _dateTimeProvider.UtcNow,
            userId);
        DbContext.BiometricConsents.Add(consent);
        AddEvent(student.Id, null, consent.Id, FaceEnrollmentEventType.ConsentAccepted, "Succeeded", null, userId, "FaceEventDescriptionConsentAccepted");

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<OperationResult> EnrollAsync(string userId, FaceEnrollmentCommand command, CancellationToken cancellationToken = default)
    {
        if (!_rateLimiter.TryAcquire(userId, _dateTimeProvider.UtcNow))
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorBiometricRateLimit"));
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var student = await DbContext.Students.FirstOrDefaultAsync(item => item.ApplicationUserId == userId && item.IsActive, cancellationToken);
        if (student is null)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var activeConsent = await DbContext.BiometricConsents.FirstOrDefaultAsync(
            item => item.StudentId == student.Id && item.IsActive,
            cancellationToken);
        if (activeConsent is null)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorBiometricConsentRequired"));
        }

        var activeTemplate = await DbContext.StudentFaceTemplates.FirstOrDefaultAsync(
            item => item.StudentId == student.Id && item.IsActive,
            cancellationToken);
        AddEvent(
            student.Id,
            activeTemplate?.Id,
            activeConsent.Id,
            activeTemplate is null ? FaceEnrollmentEventType.EnrollmentStarted : FaceEnrollmentEventType.ReEnrollmentStarted,
            "Started",
            null,
            userId,
            activeTemplate is null ? "FaceEventDescriptionEnrollmentStarted" : "FaceEventDescriptionReEnrollmentStarted");

        var processing = await _processor.ProcessAsync(command.Samples, cancellationToken);
        if (!processing.Succeeded || processing.Template is null)
        {
            AddEvent(
                student.Id,
                activeTemplate?.Id,
                activeConsent.Id,
                processing.RejectionReason is FaceCaptureRejectionReason.NoFace or FaceCaptureRejectionReason.MultipleFaces or FaceCaptureRejectionReason.LowBrightness or FaceCaptureRejectionReason.HighBrightness or FaceCaptureRejectionReason.LowSharpness or FaceCaptureRejectionReason.QualityTooLow
                    ? FaceEnrollmentEventType.CaptureRejected
                    : FaceEnrollmentEventType.EnrollmentFailed,
                "Failed",
                processing.RejectionReason.ToString(),
                userId,
                "FaceEventDescriptionCaptureRejected");

            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return OperationResult.Failure(OperationErrors.Dependency(processing.MessageKey));
        }

        var unprotectedTemplate = processing.Template.TemplateData;
        var protectedTemplate = _templateProtector.Protect(unprotectedTemplate);
        var nextVersion = await DbContext.StudentFaceTemplates
            .Where(item => item.StudentId == student.Id)
            .Select(item => (int?)item.TemplateVersion)
            .MaxAsync(cancellationToken) ?? 0;
        nextVersion++;

        if (activeTemplate is not null)
        {
            activeTemplate.Revoke(_dateTimeProvider.UtcNow, userId, "Replaced by student re-enrollment.", requiresReEnrollment: false);
            AddEvent(student.Id, activeTemplate.Id, activeConsent.Id, FaceEnrollmentEventType.TemplateReplaced, "Succeeded", null, userId, "FaceEventDescriptionPreviousTemplateReplaced");
        }

        var template = new StudentFaceTemplate(
            student.Id,
            activeConsent.Id,
            protectedTemplate,
            Fingerprint(unprotectedTemplate),
            processing.EngineName,
            processing.EngineVersion,
            processing.ModelName,
            processing.ModelVersion,
            processing.Template.Version,
            Math.Max(1, processing.Template.EncodingDimension),
            processing.QualityScore,
            processing.CaptureCount,
            nextVersion,
            _dateTimeProvider.UtcNow);

        DbContext.StudentFaceTemplates.Add(template);
        AddEvent(student.Id, template.Id, activeConsent.Id, FaceEnrollmentEventType.TemplateCreated, "Succeeded", null, userId, "FaceEventDescriptionTemplateCreated");

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<OperationResult> WithdrawConsentAsync(string userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var student = await DbContext.Students.FirstOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);
        if (student is null)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var activeConsent = await DbContext.BiometricConsents.FirstOrDefaultAsync(item => item.StudentId == student.Id && item.IsActive, cancellationToken);
        if (activeConsent is null)
        {
            return OperationResult.Success();
        }

        activeConsent.Withdraw(_dateTimeProvider.UtcNow, userId);
        AddEvent(student.Id, null, activeConsent.Id, FaceEnrollmentEventType.ConsentWithdrawn, "Succeeded", null, userId, "FaceEventDescriptionConsentWithdrawn");

        var activeTemplate = await DbContext.StudentFaceTemplates.FirstOrDefaultAsync(item => item.StudentId == student.Id && item.IsActive, cancellationToken);
        if (activeTemplate is not null)
        {
            activeTemplate.Revoke(_dateTimeProvider.UtcNow, userId, "Consent withdrawn.", requiresReEnrollment: false);
            AddEvent(student.Id, activeTemplate.Id, activeConsent.Id, FaceEnrollmentEventType.TemplateRevoked, "Succeeded", null, userId, "FaceEventDescriptionTemplateRevokedAfterConsentWithdrawal");
        }

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<PagedResult<AdminBiometricEnrollmentRowDto>> GetAdminPagedAsync(AdminBiometricEnrollmentQuery query, CancellationToken cancellationToken = default)
    {
        var rows = await (
            from student in DbContext.Students.Include(student => student.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            select new { Student = student, User = user }).ToListAsync(cancellationToken);

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            rows = rows.Where(item =>
                item.Student.StudentNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Student.NameEnglish.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                item.Student.NameArabic.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (item.User.Email ?? string.Empty).Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var items = new List<AdminBiometricEnrollmentRowDto>();
        foreach (var row in rows.OrderBy(item => item.Student.StudentNumber))
        {
            var status = await BuildStudentStatusAsync(row.Student, row.User, cancellationToken);
            var activeTemplate = await DbContext.StudentFaceTemplates.AsNoTracking()
                .Where(item => item.StudentId == row.Student.Id && item.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
            var mapped = new AdminBiometricEnrollmentRowDto(
                row.Student.Id,
                row.Student.StudentNumber,
                row.Student.NameEnglish,
                row.Student.NameArabic,
                row.User.Email ?? string.Empty,
                row.Student.Department?.NameEnglish ?? string.Empty,
                row.Student.Department?.NameArabic ?? string.Empty,
                row.Student.IsActive && row.User.IsActive && !row.User.IsDisabled,
                status.Status,
                status.EnrolledAtUtc,
                status.RevokedAtUtc,
                activeTemplate?.EngineName,
                activeTemplate?.TemplateVersion);
            items.Add(mapped);
        }

        if (query.Status.HasValue)
        {
            items = items.Where(item => item.Status == query.Status.Value).ToList();
        }

        var total = items.Count;
        var pageItems = items
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PagedResult<AdminBiometricEnrollmentRowDto>(pageItems, query.PageNumber, query.PageSize, total);
    }

    public async Task<AdminBiometricEnrollmentDetailsDto?> GetAdminDetailsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var row = await (
            from student in DbContext.Students.Include(student => student.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.Id == studentId
            select new { Student = student, User = user }).FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var status = await BuildStudentStatusAsync(row.Student, row.User, cancellationToken);
        var templates = await DbContext.StudentFaceTemplates
            .AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderByDescending(item => item.TemplateVersion)
            .Select(item => ToTemplateMetadata(item))
            .ToListAsync(cancellationToken);

        return new AdminBiometricEnrollmentDetailsDto(
            row.Student.Id,
            row.Student.StudentNumber,
            row.Student.NameEnglish,
            row.Student.NameArabic,
            row.User.Email ?? string.Empty,
            row.Student.Department?.NameEnglish ?? string.Empty,
            row.Student.Department?.NameArabic ?? string.Empty,
            row.Student.IsActive && row.User.IsActive && !row.User.IsDisabled,
            status,
            templates.FirstOrDefault(item => item.IsActive),
            templates,
            await GetEventDtosAsync(studentId, cancellationToken));
    }

    public Task<OperationResult> AdminRevokeAsync(Guid studentId, string performedByUserId, AdminBiometricActionCommand command, CancellationToken cancellationToken = default)
        => AdminRevokeInternalAsync(studentId, performedByUserId, command, requiresReEnrollment: false, FaceEnrollmentEventType.AdminRevoked, cancellationToken);

    public Task<OperationResult> AdminRequireReEnrollmentAsync(Guid studentId, string performedByUserId, AdminBiometricActionCommand command, CancellationToken cancellationToken = default)
        => AdminRevokeInternalAsync(studentId, performedByUserId, command, requiresReEnrollment: true, FaceEnrollmentEventType.TemplateRevoked, cancellationToken);

    private async Task<OperationResult> AdminRevokeInternalAsync(
        Guid studentId,
        string performedByUserId,
        AdminBiometricActionCommand command,
        bool requiresReEnrollment,
        FaceEnrollmentEventType eventType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return OperationResult.Failure(OperationErrors.Validation(nameof(command.Reason), "ValidationRequired"));
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var studentExists = await DbContext.Students.AnyAsync(item => item.Id == studentId, cancellationToken);
        if (!studentExists)
        {
            return OperationResult.Failure(OperationErrors.NotFound("ErrorStudentNotFound"));
        }

        var activeTemplate = await DbContext.StudentFaceTemplates.FirstOrDefaultAsync(item => item.StudentId == studentId && item.IsActive, cancellationToken);
        if (activeTemplate is null)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorBiometricActiveTemplateRequired"));
        }

        SetOriginalRowVersion(activeTemplate, command.RowVersion);
        activeTemplate.Revoke(_dateTimeProvider.UtcNow, performedByUserId, command.Reason, requiresReEnrollment);
        AddEvent(
            studentId,
            activeTemplate.Id,
            activeTemplate.BiometricConsentId,
            eventType,
            "Succeeded",
            null,
            performedByUserId,
            requiresReEnrollment ? "FaceEventDescriptionTemplateRequiresReEnrollment" : "FaceEventDescriptionTemplateRevokedByAdmin");

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task<StudentBiometricStatusDto> BuildStudentStatusAsync(Student student, ApplicationUser user, CancellationToken cancellationToken)
    {
        var readiness = _readinessService.GetReadiness();
        var activeConsent = await DbContext.BiometricConsents.AsNoTracking()
            .Where(item => item.StudentId == student.Id && item.IsActive)
            .OrderByDescending(item => item.AcceptedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var latestConsent = await DbContext.BiometricConsents.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .OrderByDescending(item => item.AcceptedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var activeTemplate = await DbContext.StudentFaceTemplates.AsNoTracking()
            .Where(item => item.StudentId == student.Id && item.IsActive)
            .OrderByDescending(item => item.TemplateVersion)
            .FirstOrDefaultAsync(cancellationToken);
        var latestTemplate = await DbContext.StudentFaceTemplates.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .OrderByDescending(item => item.TemplateVersion)
            .FirstOrDefaultAsync(cancellationToken);

        var status = DetermineStatus(readiness, activeConsent, latestConsent, activeTemplate, latestTemplate);
        return new StudentBiometricStatusDto(
            student.Id,
            student.StudentNumber,
            student.NameEnglish,
            student.NameArabic,
            student.IsActive && user.IsActive && !user.IsDisabled,
            status,
            activeConsent is not null,
            activeTemplate is not null,
            activeConsent?.ConsentVersion ?? latestConsent?.ConsentVersion,
            activeConsent?.AcceptedAtUtc ?? latestConsent?.AcceptedAtUtc,
            latestConsent?.WithdrawnAtUtc,
            activeTemplate?.EnrolledAtUtc,
            latestTemplate?.RevokedAtUtc,
            readiness.Mode,
            readiness.EnrollmentEnabled,
            readiness.StatusMessageKey);
    }

    private static FaceEnrollmentStatus DetermineStatus(
        FaceEngineReadinessDto readiness,
        BiometricConsent? activeConsent,
        BiometricConsent? latestConsent,
        StudentFaceTemplate? activeTemplate,
        StudentFaceTemplate? latestTemplate)
    {
        if (activeTemplate is not null)
        {
            return FaceEnrollmentStatus.Active;
        }

        if (latestConsent?.WithdrawnAtUtc is not null && activeConsent is null)
        {
            return FaceEnrollmentStatus.ConsentWithdrawn;
        }

        if (latestTemplate?.RequiresReEnrollment == true)
        {
            return FaceEnrollmentStatus.RequiresReEnrollment;
        }

        if (latestTemplate?.RevokedAtUtc is not null)
        {
            return FaceEnrollmentStatus.Revoked;
        }

        if (!readiness.EnrollmentEnabled)
        {
            return FaceEnrollmentStatus.EngineUnavailable;
        }

        return FaceEnrollmentStatus.NotEnrolled;
    }

    private async Task<IReadOnlyList<FaceEnrollmentEventDto>> GetEventDtosAsync(Guid studentId, CancellationToken cancellationToken)
        => await DbContext.FaceEnrollmentEvents
            .AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderByDescending(item => item.OccurredAtUtc)
            .Select(item => new FaceEnrollmentEventDto(
                item.OccurredAtUtc,
                item.EventType,
                item.Outcome,
                item.ErrorCode,
                item.PerformedByUserId,
                item.EngineName,
                item.EngineVersion,
                item.SafeDescription))
            .ToListAsync(cancellationToken);

    private async Task<StudentUserRow?> GetStudentRowByUserIdAsync(string userId, bool tracking, CancellationToken cancellationToken)
    {
        var students = tracking
            ? DbContext.Students.Include(student => student.Department)
            : DbContext.Students.Include(student => student.Department).AsNoTracking();
        return await (
            from student in students
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.ApplicationUserId == userId
            select new StudentUserRow(student, user)).FirstOrDefaultAsync(cancellationToken);
    }

    private void AddEvent(
        Guid studentId,
        Guid? templateId,
        Guid? consentId,
        FaceEnrollmentEventType eventType,
        string outcome,
        string? errorCode,
        string performedByUserId,
        string safeDescription)
    {
        var readiness = _readinessService.GetReadiness();
        DbContext.FaceEnrollmentEvents.Add(new FaceEnrollmentEvent(
            studentId,
            templateId,
            consentId,
            eventType,
            outcome,
            errorCode,
            _dateTimeProvider.UtcNow,
            performedByUserId,
            readiness.EngineName,
            readiness.EngineVersion,
            safeDescription));
    }

    private static FaceTemplateMetadataDto ToTemplateMetadata(StudentFaceTemplate template)
        => new(
            template.Id,
            template.EngineName,
            template.EngineVersion,
            template.ModelName,
            template.ModelVersion,
            template.TemplateFormatVersion,
            template.EmbeddingDimension,
            template.QualityScore,
            template.CaptureCount,
            template.TemplateVersion,
            template.EnrolledAtUtc,
            template.RevokedAtUtc,
            template.RevocationReason,
            template.IsActive,
            template.RequiresReEnrollment,
            RowVersion(template.RowVersion));

    private string ConsentHash()
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(_options.ConsentTextCanonical)));

    private static string Fingerprint(byte[] template)
        => Convert.ToHexString(SHA256.HashData(template));

    private sealed record StudentUserRow(Student Student, ApplicationUser User);
}
