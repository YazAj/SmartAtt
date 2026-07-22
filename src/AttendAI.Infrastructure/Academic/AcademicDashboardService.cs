using AttendAI.Application.Academic.Dashboard;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class AcademicDashboardService : IAcademicDashboardService
{
    private readonly ApplicationDbContext _dbContext;

    public AcademicDashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminDashboardSummaryDto> GetAdminSummaryAsync(CancellationToken cancellationToken = default)
    {
        return new AdminDashboardSummaryDto(
            await _dbContext.Students.CountAsync(student => student.IsActive, cancellationToken),
            await _dbContext.Instructors.CountAsync(instructor => instructor.IsActive, cancellationToken),
            await _dbContext.Departments.CountAsync(department => department.IsActive, cancellationToken),
            await _dbContext.Courses.CountAsync(course => course.IsActive, cancellationToken),
            await _dbContext.Sections.CountAsync(section => section.IsActive, cancellationToken),
            await _dbContext.Classrooms.CountAsync(classroom => classroom.IsActive, cancellationToken));
    }

    public async Task<InstructorDashboardSummaryDto> GetInstructorSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var assignments = await _dbContext.InstructorAssignments
            .AsNoTracking()
            .Include(assignment => assignment.Section)
                .ThenInclude(section => section!.Course)
            .Include(assignment => assignment.Instructor)
            .Where(assignment => assignment.IsActive && assignment.Instructor!.ApplicationUserId == userId)
            .OrderBy(assignment => assignment.Section!.AcademicYear)
            .ThenBy(assignment => assignment.Section!.Semester)
            .ThenBy(assignment => assignment.Section!.Course!.Code)
            .Select(assignment => new InstructorDashboardSectionDto(
                assignment.Section!.Course!.Code,
                assignment.Section.Course.NameEnglish,
                assignment.Section.Course.NameArabic,
                assignment.Section.SectionNumber,
                assignment.Section.AcademicYear,
                assignment.Section.Semester,
                assignment.IsPrimary))
            .ToListAsync(cancellationToken);

        return new InstructorDashboardSummaryDto(assignments);
    }

    public async Task<StudentDashboardSummaryDto> GetStudentSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var student = await _dbContext.Students
            .AsNoTracking()
            .Include(item => item.Department)
            .FirstOrDefaultAsync(item => item.ApplicationUserId == userId, cancellationToken);

        if (student is null)
        {
            return new StudentDashboardSummaryDto(null, null, null, []);
        }

        var enrollments = await _dbContext.StudentEnrollments
            .AsNoTracking()
            .Include(enrollment => enrollment.Section)
                .ThenInclude(section => section!.Course)
            .Where(enrollment =>
                enrollment.StudentId == student.Id &&
                enrollment.IsActive &&
                enrollment.EnrollmentStatus == EnrollmentStatus.Active)
            .OrderBy(enrollment => enrollment.Section!.AcademicYear)
            .ThenBy(enrollment => enrollment.Section!.Semester)
            .ThenBy(enrollment => enrollment.Section!.Course!.Code)
            .Select(enrollment => new StudentDashboardEnrollmentDto(
                enrollment.Section!.Course!.Code,
                enrollment.Section.Course.NameEnglish,
                enrollment.Section.Course.NameArabic,
                enrollment.Section.SectionNumber,
                enrollment.Section.AcademicYear,
                enrollment.Section.Semester,
                enrollment.EnrollmentStatus))
            .ToListAsync(cancellationToken);

        return new StudentDashboardSummaryDto(
            student.Department?.NameEnglish,
            student.Department?.NameArabic,
            student.AcademicLevel,
            enrollments);
    }
}
