using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.Common.Models;
using AttendAI.Application.Lectures;
using AttendAI.Application.Reporting;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Reporting;

public sealed class AttendanceReportingService : AcademicServiceBase, IAttendanceReportingService, IAttendanceReportExportService
{
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;
    private const int MaxExportRows = 10000;

    private readonly IApplicationTimeZoneService _timeZoneService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AttendanceReportingService(
        ApplicationDbContext dbContext,
        IApplicationTimeZoneService timeZoneService,
        IDateTimeProvider dateTimeProvider)
        : base(dbContext)
    {
        _timeZoneService = timeZoneService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<StudentAttendanceReportDto?> GetStudentReportAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var studentId = await ResolveStudentIdAsync(userId, cancellationToken);
        if (studentId == Guid.Empty)
        {
            return null;
        }

        var normalized = NormalizeFilter(filter, forcedStudentId: studentId);
        var rows = await GetAttendanceRowsAsync(normalized, cancellationToken);
        return new StudentAttendanceReportDto(
            BuildSummary(rows),
            Page(rows, normalized),
            normalized,
            await GetStudentCourseOptionsAsync(studentId, cancellationToken));
    }

    public async Task<InstructorAttendanceReportDto?> GetInstructorReportAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var instructorId = await ResolveInstructorIdAsync(userId, cancellationToken);
        if (instructorId == Guid.Empty)
        {
            return null;
        }

        var normalized = NormalizeFilter(filter, forcedInstructorId: instructorId);
        var rows = await GetAttendanceRowsAsync(normalized, cancellationToken);
        return new InstructorAttendanceReportDto(
            BuildSummary(rows),
            Page(rows, normalized),
            normalized,
            await GetInstructorCourseOptionsAsync(instructorId, cancellationToken),
            await GetInstructorSectionOptionsAsync(instructorId, cancellationToken));
    }

    public async Task<AdminAttendanceReportDto> GetAdminReportAsync(
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeFilter(filter);
        var rows = await GetAttendanceRowsAsync(normalized, cancellationToken);
        return new AdminAttendanceReportDto(
            BuildSummary(rows),
            Page(rows, normalized),
            normalized,
            await GetAdminFilterOptionsAsync(cancellationToken));
    }

    public async Task<AttendanceAuditReportDto> GetAdminAuditReportAsync(
        AttendanceAuditFilter filter,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeAuditFilter(filter);
        var rows = await GetAuditRowsAsync(normalized, cancellationToken);
        return new AttendanceAuditReportDto(PageAudit(rows, normalized), normalized);
    }

    public async Task<ReportFileExportDto?> ExportStudentAttendanceAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var studentId = await ResolveStudentIdAsync(userId, cancellationToken);
        if (studentId == Guid.Empty)
        {
            return null;
        }

        var normalized = NormalizeFilter(filter, forcedStudentId: studentId, forExport: true);
        var rows = await GetAttendanceRowsAsync(normalized, cancellationToken);
        return AttendanceExport("student", rows);
    }

    public async Task<ReportFileExportDto?> ExportInstructorAttendanceAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var instructorId = await ResolveInstructorIdAsync(userId, cancellationToken);
        if (instructorId == Guid.Empty)
        {
            return null;
        }

        var normalized = NormalizeFilter(filter, forcedInstructorId: instructorId, forExport: true);
        var rows = await GetAttendanceRowsAsync(normalized, cancellationToken);
        return AttendanceExport("instructor", rows);
    }

    public async Task<ReportFileExportDto> ExportAdminAttendanceAsync(
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeFilter(filter, forExport: true);
        var rows = await GetAttendanceRowsAsync(normalized, cancellationToken);
        return AttendanceExport("admin", rows);
    }

    public async Task<ReportFileExportDto> ExportAdminAuditAsync(
        AttendanceAuditFilter filter,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeAuditFilter(filter, forExport: true);
        var rows = await GetAuditRowsAsync(normalized, cancellationToken);
        return new ReportFileExportDto(
            FileName("admin-audit"),
            SafeCsvReportWriter.ContentType,
            SafeCsvReportWriter.BuildAuditCsv(rows.Take(MaxExportRows)));
    }

    private async Task<IReadOnlyList<AttendanceReportRowDto>> GetAttendanceRowsAsync(
        AttendanceReportFilter filter,
        CancellationToken cancellationToken)
    {
        var query =
            from enrollment in DbContext.StudentEnrollments.AsNoTracking()
            join student in DbContext.Students.AsNoTracking() on enrollment.StudentId equals student.Id
            join section in DbContext.Sections.AsNoTracking() on enrollment.SectionId equals section.Id
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            join session in DbContext.LectureSessions.AsNoTracking() on section.Id equals session.SectionId
            join instructor in DbContext.Instructors.AsNoTracking() on session.InstructorId equals instructor.Id
            join classroom in DbContext.Classrooms.AsNoTracking() on session.ClassroomId equals classroom.Id
            where enrollment.IsActive &&
                  enrollment.EnrollmentStatus == EnrollmentStatus.Active &&
                  student.IsActive &&
                  section.IsActive &&
                  course.IsActive &&
                  session.AttendanceCheckInEnabled &&
                  session.Status != LectureSessionStatus.Cancelled
            select new RawAttendanceRow(
                session.Id,
                student.Id,
                student.StudentNumber,
                student.NameEnglish,
                student.NameArabic,
                course.Id,
                course.Code,
                course.NameEnglish,
                course.NameArabic,
                section.Id,
                section.SectionNumber,
                instructor.Id,
                instructor.NameEnglish,
                instructor.NameArabic,
                classroom.Code,
                session.SessionDate,
                session.ScheduledStartUtc,
                session.ScheduledEndUtc);

        var rawRows = await query.ToListAsync(cancellationToken);
        var nowUtc = _dateTimeProvider.UtcNow;
        var filteredRows = rawRows
            .Where(row => !filter.StudentId.HasValue || row.StudentId == filter.StudentId.Value)
            .Where(row => !filter.InstructorId.HasValue || row.InstructorId == filter.InstructorId.Value)
            .Where(row => !filter.CourseId.HasValue || row.CourseId == filter.CourseId.Value)
            .Where(row => !filter.SectionId.HasValue || row.SectionId == filter.SectionId.Value)
            .Where(row => !filter.LectureSessionId.HasValue || row.LectureSessionId == filter.LectureSessionId.Value)
            .Where(row => row.ScheduledEndUtc <= nowUtc)
            .Where(row => !filter.FromDate.HasValue || row.SessionDate >= filter.FromDate.Value)
            .Where(row => !filter.ToDate.HasValue || row.SessionDate <= filter.ToDate.Value);

        var search = NormalizeText(filter.Search);
        if (search.Length > 0)
        {
            filteredRows = filteredRows.Where(row =>
                Contains(row.StudentNumber, search) ||
                Contains(row.StudentNameEnglish, search) ||
                Contains(row.StudentNameArabic, search) ||
                Contains(row.CourseCode, search) ||
                Contains(row.CourseNameEnglish, search) ||
                Contains(row.CourseNameArabic, search) ||
                Contains(row.SectionNumber, search) ||
                Contains(row.InstructorNameEnglish, search) ||
                Contains(row.InstructorNameArabic, search) ||
                Contains(row.ClassroomCode, search));
        }

        var materializedRows = filteredRows.ToList();
        var sessionIds = materializedRows.Select(row => row.LectureSessionId).Distinct().ToList();
        var studentIds = materializedRows.Select(row => row.StudentId).Distinct().ToList();
        var records = sessionIds.Count == 0 || studentIds.Count == 0
            ? []
            : await DbContext.AttendanceRecords
                .AsNoTracking()
                .Where(record => sessionIds.Contains(record.LectureSessionId) && studentIds.Contains(record.StudentId))
                .Select(record => new AttendanceRecordLookup(
                    record.LectureSessionId,
                    record.StudentId,
                    record.Status,
                    record.CheckedInAtUtc))
                .ToListAsync(cancellationToken);

        var recordLookup = records
            .GroupBy(record => (record.LectureSessionId, record.StudentId))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(record => record.CheckedInAtUtc).First());

        var rows = materializedRows.Select(row =>
        {
            recordLookup.TryGetValue((row.LectureSessionId, row.StudentId), out var record);
            var reportStatus = record?.Status switch
            {
                AttendanceStatus.Present => AttendanceReportStatus.Present,
                AttendanceStatus.Late => AttendanceReportStatus.Late,
                _ => AttendanceReportStatus.Missed
            };

            return new AttendanceReportRowDto(
                row.LectureSessionId,
                row.StudentId,
                row.StudentNumber,
                row.StudentNameEnglish,
                row.StudentNameArabic,
                row.CourseCode,
                row.CourseNameEnglish,
                row.CourseNameArabic,
                row.SectionNumber,
                row.InstructorNameEnglish,
                row.InstructorNameArabic,
                row.ClassroomCode,
                row.SessionDate,
                _timeZoneService.ConvertUtcToLocal(row.ScheduledStartUtc),
                _timeZoneService.ConvertUtcToLocal(row.ScheduledEndUtc),
                reportStatus,
                record?.Status,
                record is null ? null : _timeZoneService.ConvertUtcToLocal(record.CheckedInAtUtc));
        });

        if (filter.Status != AttendanceReportStatus.All)
        {
            rows = rows.Where(row => row.ReportStatus == filter.Status);
        }

        return ApplySort(rows, filter.Sort).ToList();
    }

    private async Task<IReadOnlyList<AttendanceAuditEventDto>> GetAuditRowsAsync(
        AttendanceAuditFilter filter,
        CancellationToken cancellationToken)
    {
        var rows = new List<AuditRawRow>();

        if (filter.Category is AuditEventCategory.All or AuditEventCategory.LectureSession)
        {
            rows.AddRange(await GetLectureAuditRowsAsync(cancellationToken));
        }

        if (filter.Category is AuditEventCategory.All or AuditEventCategory.Attendance)
        {
            rows.AddRange(await GetAttendanceAuditRowsAsync(cancellationToken));
        }

        if (filter.Category is AuditEventCategory.All or AuditEventCategory.FaceVerification)
        {
            rows.AddRange(await GetFaceVerificationAuditRowsAsync(cancellationToken));
        }

        if (filter.Category is AuditEventCategory.All or AuditEventCategory.FaceEnrollment)
        {
            rows.AddRange(await GetFaceEnrollmentAuditRowsAsync(cancellationToken));
        }

        var filteredRows = rows.AsEnumerable()
            .Where(row => IsInAuditDateRange(row, filter));

        var search = NormalizeText(filter.Search);
        if (search.Length > 0)
        {
            filteredRows = filteredRows.Where(row =>
                Contains(row.EventType, search) ||
                Contains(row.Actor, search) ||
                Contains(row.Subject, search) ||
                Contains(row.Outcome, search) ||
                Contains(row.SafeDescription, search));
        }

        return filteredRows
            .OrderByDescending(row => row.OccurredAtUtc)
            .ThenByDescending(row => row.Id)
            .Select(row => new AttendanceAuditEventDto(
                row.Id,
                _timeZoneService.ConvertUtcToLocal(row.OccurredAtUtc),
                row.Category,
                row.EventType,
                row.Actor,
                row.Subject,
                row.Outcome,
                row.SafeDescription))
            .ToList();
    }

    private async Task<IReadOnlyList<AuditRawRow>> GetLectureAuditRowsAsync(CancellationToken cancellationToken)
        => await (
            from item in DbContext.LectureSessionEvents.AsNoTracking()
            join session in DbContext.LectureSessions.AsNoTracking() on item.LectureSessionId equals session.Id
            join section in DbContext.Sections.AsNoTracking() on session.SectionId equals section.Id
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            select new AuditRawRow(
                item.Id,
                item.OccurredAtUtc,
                AuditEventCategory.LectureSession,
                item.EventType.ToString(),
                item.PerformedByUserId == string.Empty ? "System" : item.PerformedByUserId,
                course.Code + " " + section.SectionNumber,
                (item.PreviousStatus.HasValue ? item.PreviousStatus.Value.ToString() : "None") + " -> " +
                (item.NewStatus.HasValue ? item.NewStatus.Value.ToString() : "None"),
                item.SafeDescription))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<AuditRawRow>> GetAttendanceAuditRowsAsync(CancellationToken cancellationToken)
        => await (
            from item in DbContext.AttendanceAttempts.AsNoTracking()
            join student in DbContext.Students.AsNoTracking() on item.StudentId equals student.Id
            join session in DbContext.LectureSessions.AsNoTracking() on item.LectureSessionId equals session.Id
            join section in DbContext.Sections.AsNoTracking() on session.SectionId equals section.Id
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            select new AuditRawRow(
                item.Id,
                item.AttemptedAtUtc,
                AuditEventCategory.Attendance,
                item.Outcome.ToString(),
                student.StudentNumber + " - " + student.NameEnglish,
                course.Code + " " + section.SectionNumber,
                item.FailureReason == AttendanceFailureReason.None
                    ? item.Outcome.ToString()
                    : item.FailureReason.ToString(),
                item.SafeDescription))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<AuditRawRow>> GetFaceVerificationAuditRowsAsync(CancellationToken cancellationToken)
        => await (
            from item in DbContext.FaceVerificationAttempts.AsNoTracking()
            join student in DbContext.Students.AsNoTracking() on item.StudentId equals student.Id
            select new AuditRawRow(
                item.Id,
                item.AttemptedAtUtc,
                AuditEventCategory.FaceVerification,
                item.VerificationPurpose.ToString(),
                student.StudentNumber + " - " + student.NameEnglish,
                item.ModelName,
                item.Outcome.ToString() + " / " + item.Decision.ToString(),
                item.SafeDescription))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<AuditRawRow>> GetFaceEnrollmentAuditRowsAsync(CancellationToken cancellationToken)
        => await (
            from item in DbContext.FaceEnrollmentEvents.AsNoTracking()
            join student in DbContext.Students.AsNoTracking() on item.StudentId equals student.Id
            select new AuditRawRow(
                item.Id,
                item.OccurredAtUtc,
                AuditEventCategory.FaceEnrollment,
                item.EventType.ToString(),
                item.PerformedByUserId == string.Empty ? "System" : item.PerformedByUserId,
                student.StudentNumber + " - " + student.NameEnglish,
                item.Outcome,
                item.SafeDescription))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<ReportFilterOptionDto>> GetStudentCourseOptionsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var rows = await (
            from enrollment in DbContext.StudentEnrollments.AsNoTracking()
            join section in DbContext.Sections.AsNoTracking() on enrollment.SectionId equals section.Id
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            where enrollment.StudentId == studentId && enrollment.IsActive && section.IsActive && course.IsActive
            orderby course.Code
            select new ReportFilterOptionDto(course.Id, course.Code + " - " + course.NameEnglish, course.Code + " - " + course.NameArabic))
            .ToListAsync(cancellationToken);

        return rows.DistinctBy(row => row.Id).ToList();
    }

    private async Task<IReadOnlyList<ReportFilterOptionDto>> GetInstructorCourseOptionsAsync(Guid instructorId, CancellationToken cancellationToken)
    {
        var rows = await (
            from session in DbContext.LectureSessions.AsNoTracking()
            join section in DbContext.Sections.AsNoTracking() on session.SectionId equals section.Id
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            where session.InstructorId == instructorId && section.IsActive && course.IsActive
            orderby course.Code
            select new ReportFilterOptionDto(course.Id, course.Code + " - " + course.NameEnglish, course.Code + " - " + course.NameArabic))
            .ToListAsync(cancellationToken);

        return rows.DistinctBy(row => row.Id).ToList();
    }

    private async Task<IReadOnlyList<ReportFilterOptionDto>> GetInstructorSectionOptionsAsync(Guid instructorId, CancellationToken cancellationToken)
    {
        var rows = await (
            from session in DbContext.LectureSessions.AsNoTracking()
            join section in DbContext.Sections.AsNoTracking() on session.SectionId equals section.Id
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            where session.InstructorId == instructorId && section.IsActive && course.IsActive
            orderby course.Code, section.SectionNumber
            select new ReportFilterOptionDto(section.Id, course.Code + " - " + section.SectionNumber, course.Code + " - " + section.SectionNumber))
            .ToListAsync(cancellationToken);

        return rows.DistinctBy(row => row.Id).ToList();
    }

    private async Task<AttendanceReportFilterOptionsDto> GetAdminFilterOptionsAsync(CancellationToken cancellationToken)
        => new(
            await GetAdminCourseOptionsAsync(cancellationToken),
            await GetAdminSectionOptionsAsync(cancellationToken),
            await GetAdminInstructorOptionsAsync(cancellationToken));

    private async Task<IReadOnlyList<ReportFilterOptionDto>> GetAdminCourseOptionsAsync(CancellationToken cancellationToken)
        => await DbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsActive)
            .OrderBy(course => course.Code)
            .Select(course => new ReportFilterOptionDto(course.Id, course.Code + " - " + course.NameEnglish, course.Code + " - " + course.NameArabic))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<ReportFilterOptionDto>> GetAdminSectionOptionsAsync(CancellationToken cancellationToken)
        => await (
            from section in DbContext.Sections.AsNoTracking()
            join course in DbContext.Courses.AsNoTracking() on section.CourseId equals course.Id
            where section.IsActive && course.IsActive
            orderby course.Code, section.SectionNumber
            select new ReportFilterOptionDto(section.Id, course.Code + " - " + section.SectionNumber, course.Code + " - " + section.SectionNumber))
            .ToListAsync(cancellationToken);

    private async Task<IReadOnlyList<ReportFilterOptionDto>> GetAdminInstructorOptionsAsync(CancellationToken cancellationToken)
        => await DbContext.Instructors
            .AsNoTracking()
            .Where(instructor => instructor.IsActive)
            .OrderBy(instructor => instructor.EmployeeNumber)
            .Select(instructor => new ReportFilterOptionDto(
                instructor.Id,
                instructor.EmployeeNumber + " - " + instructor.NameEnglish,
                instructor.EmployeeNumber + " - " + instructor.NameArabic))
            .ToListAsync(cancellationToken);

    private async Task<Guid> ResolveStudentIdAsync(string userId, CancellationToken cancellationToken)
        => await DbContext.Students
            .AsNoTracking()
            .Where(student => student.ApplicationUserId == userId && student.IsActive)
            .Select(student => student.Id)
            .FirstOrDefaultAsync(cancellationToken);

    private async Task<Guid> ResolveInstructorIdAsync(string userId, CancellationToken cancellationToken)
        => await DbContext.Instructors
            .AsNoTracking()
            .Where(instructor => instructor.ApplicationUserId == userId && instructor.IsActive)
            .Select(instructor => instructor.Id)
            .FirstOrDefaultAsync(cancellationToken);

    private static AttendanceReportSummaryDto BuildSummary(IReadOnlyList<AttendanceReportRowDto> rows)
    {
        var present = rows.Count(row => row.ReportStatus == AttendanceReportStatus.Present);
        var late = rows.Count(row => row.ReportStatus == AttendanceReportStatus.Late);
        var missed = rows.Count(row => row.ReportStatus == AttendanceReportStatus.Missed);
        return new AttendanceReportSummaryDto(
            rows.Count,
            present,
            late,
            missed,
            AttendanceReportCalculator.CalculatePercentage(present, late, rows.Count));
    }

    private static PagedResult<AttendanceReportRowDto> Page(
        IReadOnlyList<AttendanceReportRowDto> rows,
        AttendanceReportFilter filter)
        => new(
            rows.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            filter.PageNumber,
            filter.PageSize,
            rows.Count);

    private static PagedResult<AttendanceAuditEventDto> PageAudit(
        IReadOnlyList<AttendanceAuditEventDto> rows,
        AttendanceAuditFilter filter)
        => new(
            rows.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList(),
            filter.PageNumber,
            filter.PageSize,
            rows.Count);

    private static IEnumerable<AttendanceReportRowDto> ApplySort(
        IEnumerable<AttendanceReportRowDto> rows,
        AttendanceReportSort sort)
        => sort switch
        {
            AttendanceReportSort.SessionDateAsc => rows
                .OrderBy(row => row.ScheduledStartLocal)
                .ThenBy(row => row.StudentNumber),
            AttendanceReportSort.StudentNameAsc => rows
                .OrderBy(row => row.StudentNameEnglish)
                .ThenBy(row => row.ScheduledStartLocal),
            AttendanceReportSort.StudentNameDesc => rows
                .OrderByDescending(row => row.StudentNameEnglish)
                .ThenByDescending(row => row.ScheduledStartLocal),
            AttendanceReportSort.CourseCodeAsc => rows
                .OrderBy(row => row.CourseCode)
                .ThenBy(row => row.ScheduledStartLocal)
                .ThenBy(row => row.StudentNumber),
            AttendanceReportSort.CourseCodeDesc => rows
                .OrderByDescending(row => row.CourseCode)
                .ThenByDescending(row => row.ScheduledStartLocal)
                .ThenBy(row => row.StudentNumber),
            _ => rows
                .OrderByDescending(row => row.ScheduledStartLocal)
                .ThenBy(row => row.StudentNumber)
        };

    private bool IsInAuditDateRange(AuditRawRow row, AttendanceAuditFilter filter)
    {
        var date = DateOnly.FromDateTime(_timeZoneService.ConvertUtcToLocal(row.OccurredAtUtc).DateTime);
        return (!filter.FromDate.HasValue || date >= filter.FromDate.Value) &&
               (!filter.ToDate.HasValue || date <= filter.ToDate.Value);
    }

    private static AttendanceReportFilter NormalizeFilter(
        AttendanceReportFilter? filter,
        Guid? forcedStudentId = null,
        Guid? forcedInstructorId = null,
        bool forExport = false)
    {
        var source = filter ?? new AttendanceReportFilter();
        var (fromDate, toDate) = NormalizeDateRange(source.FromDate, source.ToDate);
        return new AttendanceReportFilter
        {
            FromDate = fromDate,
            ToDate = toDate,
            CourseId = source.CourseId,
            SectionId = source.SectionId,
            LectureSessionId = source.LectureSessionId,
            InstructorId = forcedInstructorId ?? source.InstructorId,
            StudentId = forcedStudentId ?? source.StudentId,
            Search = NormalizeText(source.Search),
            Status = Enum.IsDefined(source.Status) ? source.Status : AttendanceReportStatus.All,
            Sort = Enum.IsDefined(source.Sort) ? source.Sort : AttendanceReportSort.SessionDateDesc,
            PageNumber = forExport ? 1 : Math.Max(1, source.PageNumber),
            PageSize = forExport ? MaxExportRows : ClampPageSize(source.PageSize)
        };
    }

    private static AttendanceAuditFilter NormalizeAuditFilter(AttendanceAuditFilter? filter, bool forExport = false)
    {
        var source = filter ?? new AttendanceAuditFilter();
        var (fromDate, toDate) = NormalizeDateRange(source.FromDate, source.ToDate);
        return new AttendanceAuditFilter
        {
            FromDate = fromDate,
            ToDate = toDate,
            Category = Enum.IsDefined(source.Category) ? source.Category : AuditEventCategory.All,
            Search = NormalizeText(source.Search),
            PageNumber = forExport ? 1 : Math.Max(1, source.PageNumber),
            PageSize = forExport ? MaxExportRows : ClampPageSize(source.PageSize)
        };
    }

    private static (DateOnly? FromDate, DateOnly? ToDate) NormalizeDateRange(DateOnly? fromDate, DateOnly? toDate)
        => fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value
            ? (toDate, fromDate)
            : (fromDate, toDate);

    private static int ClampPageSize(int pageSize)
        => pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };

    private static bool Contains(string value, string search)
        => value.Contains(search, StringComparison.OrdinalIgnoreCase);

    private ReportFileExportDto AttendanceExport(string audience, IReadOnlyList<AttendanceReportRowDto> rows)
        => new(
            FileName(audience + "-attendance"),
            SafeCsvReportWriter.ContentType,
            SafeCsvReportWriter.BuildAttendanceCsv(rows.Take(MaxExportRows)));

    private string FileName(string reportName)
        => $"attendai-{reportName}-{_timeZoneService.GetLocalDate(_dateTimeProvider.UtcNow):yyyyMMdd}.csv";

    private sealed record RawAttendanceRow(
        Guid LectureSessionId,
        Guid StudentId,
        string StudentNumber,
        string StudentNameEnglish,
        string StudentNameArabic,
        Guid CourseId,
        string CourseCode,
        string CourseNameEnglish,
        string CourseNameArabic,
        Guid SectionId,
        string SectionNumber,
        Guid InstructorId,
        string InstructorNameEnglish,
        string InstructorNameArabic,
        string ClassroomCode,
        DateOnly SessionDate,
        DateTimeOffset ScheduledStartUtc,
        DateTimeOffset ScheduledEndUtc);

    private sealed record AttendanceRecordLookup(
        Guid LectureSessionId,
        Guid StudentId,
        AttendanceStatus Status,
        DateTimeOffset CheckedInAtUtc);

    private sealed record AuditRawRow(
        Guid Id,
        DateTimeOffset OccurredAtUtc,
        AuditEventCategory Category,
        string EventType,
        string Actor,
        string Subject,
        string Outcome,
        string SafeDescription);
}
