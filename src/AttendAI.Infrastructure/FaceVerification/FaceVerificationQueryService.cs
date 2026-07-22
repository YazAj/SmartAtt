using AttendAI.Application.Common.Models;
using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.FaceVerification;

public sealed class FaceVerificationQueryService : AcademicServiceBase, IFaceVerificationQueryService
{
    private readonly IFaceVerificationEligibilityService _eligibilityService;
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;

    public FaceVerificationQueryService(
        ApplicationDbContext dbContext,
        IFaceVerificationEligibilityService eligibilityService,
        IFaceEngineDiagnosticsService diagnosticsService)
        : base(dbContext)
    {
        _eligibilityService = eligibilityService;
        _diagnosticsService = diagnosticsService;
    }

    public async Task<StudentFaceVerificationStatusDto> GetStudentStatusAsync(string userId, CancellationToken cancellationToken = default)
        => new(
            await _eligibilityService.GetEligibilityAsync(userId, cancellationToken),
            await GetStudentAttemptsAsync(userId, cancellationToken),
            _diagnosticsService.GetDiagnostics());

    public async Task<IReadOnlyList<FaceVerificationAttemptDto>> GetStudentAttemptsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var studentId = await DbContext.Students
            .AsNoTracking()
            .Where(student => student.ApplicationUserId == userId)
            .Select(student => student.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return studentId == Guid.Empty
            ? []
            : DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite"
                ? (await DbContext.FaceVerificationAttempts
                    .AsNoTracking()
                    .Where(attempt => attempt.StudentId == studentId)
                    .ToListAsync(cancellationToken))
                    .OrderByDescending(attempt => attempt.AttemptedAtUtc)
                    .Take(10)
                    .Select(ToAttemptDto)
                    .ToList()
                : await DbContext.FaceVerificationAttempts
                .AsNoTracking()
                .Where(attempt => attempt.StudentId == studentId)
                .OrderByDescending(attempt => attempt.AttemptedAtUtc)
                .Take(10)
                .Select(attempt => ToAttemptDto(attempt))
                .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<AdminFaceVerificationAttemptRowDto>> GetAdminAttemptsAsync(
        AdminFaceVerificationAttemptQuery query,
        CancellationToken cancellationToken = default)
    {
        var attempts = from attempt in DbContext.FaceVerificationAttempts.AsNoTracking()
                       join student in DbContext.Students.AsNoTracking() on attempt.StudentId equals student.Id
                       join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
                       join department in DbContext.Departments.AsNoTracking() on student.DepartmentId equals department.Id
                       select new { Attempt = attempt, Student = student, User = user, Department = department };

        if (DbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            var rows = await attempts.ToListAsync(cancellationToken);
            var filteredRows = rows.AsEnumerable();
            var sqliteSearch = NormalizeText(query.Search);
            if (sqliteSearch.Length > 0)
            {
                filteredRows = filteredRows.Where(item =>
                    item.Student.StudentNumber.Contains(sqliteSearch, StringComparison.OrdinalIgnoreCase) ||
                    item.Student.NameEnglish.Contains(sqliteSearch, StringComparison.OrdinalIgnoreCase) ||
                    item.Student.NameArabic.Contains(sqliteSearch, StringComparison.OrdinalIgnoreCase) ||
                    (item.User.Email ?? string.Empty).Contains(sqliteSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (query.Outcome.HasValue)
            {
                filteredRows = filteredRows.Where(item => item.Attempt.Outcome == query.Outcome.Value);
            }

            if (query.Decision.HasValue)
            {
                filteredRows = filteredRows.Where(item => item.Attempt.Decision == query.Decision.Value);
            }

            if (query.Purpose.HasValue)
            {
                filteredRows = filteredRows.Where(item => item.Attempt.VerificationPurpose == query.Purpose.Value);
            }

            var materialized = filteredRows
                .OrderByDescending(item => item.Attempt.AttemptedAtUtc)
                .ToList();
            var sqliteItems = materialized
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(item => new AdminFaceVerificationAttemptRowDto(
                    item.Attempt.Id,
                    item.Student.Id,
                    item.Student.StudentNumber,
                    item.Student.NameEnglish,
                    item.Student.NameArabic,
                    item.User.Email ?? string.Empty,
                    item.Department.NameEnglish,
                    item.Department.NameArabic,
                    item.Attempt.VerificationPurpose,
                    item.Attempt.Outcome,
                    item.Attempt.Decision,
                    item.Attempt.Score,
                    item.Attempt.Threshold,
                    item.Attempt.ScoreMetric,
                    item.Attempt.EngineName,
                    item.Attempt.ModelName,
                    item.Attempt.AttemptedAtUtc,
                    item.Attempt.ProcessingDurationMilliseconds))
                .ToList();

            return new PagedResult<AdminFaceVerificationAttemptRowDto>(sqliteItems, query.PageNumber, query.PageSize, materialized.Count);
        }

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            attempts = attempts.Where(item =>
                item.Student.StudentNumber.Contains(search) ||
                item.Student.NameEnglish.Contains(search) ||
                item.Student.NameArabic.Contains(search) ||
                (item.User.Email ?? string.Empty).Contains(search));
        }

        if (query.Outcome.HasValue)
        {
            attempts = attempts.Where(item => item.Attempt.Outcome == query.Outcome.Value);
        }

        if (query.Decision.HasValue)
        {
            attempts = attempts.Where(item => item.Attempt.Decision == query.Decision.Value);
        }

        if (query.Purpose.HasValue)
        {
            attempts = attempts.Where(item => item.Attempt.VerificationPurpose == query.Purpose.Value);
        }

        var total = await attempts.CountAsync(cancellationToken);
        var items = await attempts
            .OrderByDescending(item => item.Attempt.AttemptedAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(item => new AdminFaceVerificationAttemptRowDto(
                item.Attempt.Id,
                item.Student.Id,
                item.Student.StudentNumber,
                item.Student.NameEnglish,
                item.Student.NameArabic,
                item.User.Email ?? string.Empty,
                item.Department.NameEnglish,
                item.Department.NameArabic,
                item.Attempt.VerificationPurpose,
                item.Attempt.Outcome,
                item.Attempt.Decision,
                item.Attempt.Score,
                item.Attempt.Threshold,
                item.Attempt.ScoreMetric,
                item.Attempt.EngineName,
                item.Attempt.ModelName,
                item.Attempt.AttemptedAtUtc,
                item.Attempt.ProcessingDurationMilliseconds))
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminFaceVerificationAttemptRowDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<AdminFaceVerificationAttemptDetailsDto?> GetAdminAttemptDetailsAsync(
        Guid attemptId,
        CancellationToken cancellationToken = default)
    {
        var row = await (
            from attempt in DbContext.FaceVerificationAttempts.AsNoTracking()
            join student in DbContext.Students.AsNoTracking() on attempt.StudentId equals student.Id
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            join department in DbContext.Departments.AsNoTracking() on student.DepartmentId equals department.Id
            where attempt.Id == attemptId
            select new AdminFaceVerificationAttemptRowDto(
                attempt.Id,
                student.Id,
                student.StudentNumber,
                student.NameEnglish,
                student.NameArabic,
                user.Email ?? string.Empty,
                department.NameEnglish,
                department.NameArabic,
                attempt.VerificationPurpose,
                attempt.Outcome,
                attempt.Decision,
                attempt.Score,
                attempt.Threshold,
                attempt.ScoreMetric,
                attempt.EngineName,
                attempt.ModelName,
                attempt.AttemptedAtUtc,
                attempt.ProcessingDurationMilliseconds))
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var metadata = await DbContext.FaceVerificationAttempts
            .AsNoTracking()
            .Where(item => item.Id == attemptId)
            .Select(item => ToAttemptDto(item))
            .FirstAsync(cancellationToken);

        return new AdminFaceVerificationAttemptDetailsDto(row, metadata);
    }

    private static FaceVerificationAttemptDto ToAttemptDto(FaceVerificationAttempt attempt)
        => new(
            attempt.Id,
            attempt.StudentId,
            attempt.FaceTemplateId,
            attempt.VerificationPurpose,
            attempt.Outcome,
            attempt.Decision,
            attempt.ErrorCode,
            attempt.Score,
            attempt.Threshold,
            attempt.ScoreMetric,
            attempt.EngineName,
            attempt.EngineVersion,
            attempt.ModelName,
            attempt.ModelVersion,
            attempt.TemplateFormatVersion,
            attempt.AttemptedAtUtc,
            attempt.ClientRequestId,
            attempt.SafeDescription,
            attempt.ImageWidth,
            attempt.ImageHeight,
            attempt.DetectedFaceCount,
            attempt.QualityScore,
            attempt.ProcessingDurationMilliseconds);
}
