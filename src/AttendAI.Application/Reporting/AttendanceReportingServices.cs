namespace AttendAI.Application.Reporting;

public interface IAttendanceReportingService
{
    Task<StudentAttendanceReportDto?> GetStudentReportAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default);

    Task<InstructorAttendanceReportDto?> GetInstructorReportAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default);

    Task<AdminAttendanceReportDto> GetAdminReportAsync(
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default);

    Task<AttendanceAuditReportDto> GetAdminAuditReportAsync(
        AttendanceAuditFilter filter,
        CancellationToken cancellationToken = default);
}

public interface IAttendanceReportExportService
{
    Task<ReportFileExportDto?> ExportStudentAttendanceAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default);

    Task<ReportFileExportDto?> ExportInstructorAttendanceAsync(
        string userId,
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default);

    Task<ReportFileExportDto> ExportAdminAttendanceAsync(
        AttendanceReportFilter filter,
        CancellationToken cancellationToken = default);

    Task<ReportFileExportDto> ExportAdminAuditAsync(
        AttendanceAuditFilter filter,
        CancellationToken cancellationToken = default);
}
