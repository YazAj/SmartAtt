using AttendAI.Domain.Enums;

namespace AttendAI.Application.Academic.Dashboard;

public sealed record AdminDashboardSummaryDto(
    int ActiveStudents,
    int ActiveInstructors,
    int ActiveDepartments,
    int ActiveCourses,
    int ActiveSections,
    int ActiveClassrooms);

public sealed record InstructorDashboardSectionDto(
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string SectionNumber,
    string AcademicYear,
    Semester Semester,
    bool IsPrimary);

public sealed record InstructorDashboardSummaryDto(IReadOnlyList<InstructorDashboardSectionDto> AssignedSections);

public sealed record StudentDashboardEnrollmentDto(
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string SectionNumber,
    string AcademicYear,
    Semester Semester,
    EnrollmentStatus EnrollmentStatus);

public sealed record StudentDashboardSummaryDto(
    string? DepartmentNameEnglish,
    string? DepartmentNameArabic,
    string? AcademicLevel,
    IReadOnlyList<StudentDashboardEnrollmentDto> EnrolledSections);

public interface IAcademicDashboardService
{
    Task<AdminDashboardSummaryDto> GetAdminSummaryAsync(CancellationToken cancellationToken = default);

    Task<InstructorDashboardSummaryDto> GetInstructorSummaryAsync(string userId, CancellationToken cancellationToken = default);

    Task<StudentDashboardSummaryDto> GetStudentSummaryAsync(string userId, CancellationToken cancellationToken = default);
}
