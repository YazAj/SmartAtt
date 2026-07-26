using AttendAI.Application.Common.Models;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Reporting;

public enum AttendanceReportStatus
{
    All = 0,
    Present = 1,
    Late = 2,
    Missed = 3
}

public enum AttendanceReportSort
{
    SessionDateDesc = 0,
    SessionDateAsc = 1,
    StudentNameAsc = 2,
    StudentNameDesc = 3,
    CourseCodeAsc = 4,
    CourseCodeDesc = 5
}

public enum AuditEventCategory
{
    All = 0,
    LectureSession = 1,
    Attendance = 2,
    FaceVerification = 3,
    FaceEnrollment = 4
}

public sealed class AttendanceReportFilter
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public Guid? CourseId { get; init; }

    public Guid? SectionId { get; init; }

    public Guid? LectureSessionId { get; init; }

    public Guid? InstructorId { get; init; }

    public Guid? StudentId { get; init; }

    public string? Search { get; init; }

    public AttendanceReportStatus Status { get; init; } = AttendanceReportStatus.All;

    public AttendanceReportSort Sort { get; init; } = AttendanceReportSort.SessionDateDesc;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public sealed class AttendanceAuditFilter
{
    public DateOnly? FromDate { get; init; }

    public DateOnly? ToDate { get; init; }

    public AuditEventCategory Category { get; init; } = AuditEventCategory.All;

    public string? Search { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public sealed record ReportFilterOptionDto(
    Guid Id,
    string LabelEnglish,
    string LabelArabic);

public sealed record AttendanceReportFilterOptionsDto(
    IReadOnlyList<ReportFilterOptionDto> Courses,
    IReadOnlyList<ReportFilterOptionDto> Sections,
    IReadOnlyList<ReportFilterOptionDto> Instructors);

public sealed record AttendanceReportSummaryDto(
    int EligibleSessionCount,
    int PresentCount,
    int LateCount,
    int MissedCount,
    decimal AttendancePercentage)
{
    public int AttendedCount => PresentCount + LateCount;
}

public sealed record AttendanceReportRowDto(
    Guid LectureSessionId,
    Guid StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string SectionNumber,
    string InstructorNameEnglish,
    string InstructorNameArabic,
    string ClassroomCode,
    DateOnly SessionDate,
    DateTimeOffset ScheduledStartLocal,
    DateTimeOffset ScheduledEndLocal,
    AttendanceReportStatus ReportStatus,
    AttendanceStatus? RecordedStatus,
    DateTimeOffset? CheckedInAtLocal);

public sealed record StudentAttendanceReportDto(
    AttendanceReportSummaryDto Summary,
    PagedResult<AttendanceReportRowDto> Rows,
    AttendanceReportFilter Filter,
    IReadOnlyList<ReportFilterOptionDto> Courses);

public sealed record InstructorAttendanceReportDto(
    AttendanceReportSummaryDto Summary,
    PagedResult<AttendanceReportRowDto> Rows,
    AttendanceReportFilter Filter,
    IReadOnlyList<ReportFilterOptionDto> Courses,
    IReadOnlyList<ReportFilterOptionDto> Sections);

public sealed record AdminAttendanceReportDto(
    AttendanceReportSummaryDto Summary,
    PagedResult<AttendanceReportRowDto> Rows,
    AttendanceReportFilter Filter,
    AttendanceReportFilterOptionsDto FilterOptions);

public sealed record AttendanceAuditEventDto(
    Guid Id,
    DateTimeOffset OccurredAtLocal,
    AuditEventCategory Category,
    string EventType,
    string Actor,
    string Subject,
    string Outcome,
    string SafeDescription);

public sealed record AttendanceAuditReportDto(
    PagedResult<AttendanceAuditEventDto> Events,
    AttendanceAuditFilter Filter);

public sealed record ReportFileExportDto(
    string FileName,
    string ContentType,
    byte[] Content);
