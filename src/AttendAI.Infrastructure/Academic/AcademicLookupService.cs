using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class AcademicLookupService : IAcademicLookupService
{
    private readonly ApplicationDbContext _dbContext;

    public AcademicLookupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LookupItem>> GetActiveDepartmentsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Departments
            .AsNoTracking()
            .Where(department => department.IsActive)
            .OrderBy(department => department.Code)
            .Select(department => new LookupItem(department.Id.ToString(), department.Code + " - " + department.NameEnglish + " / " + department.NameArabic))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LookupItem>> GetActiveCoursesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Courses
            .AsNoTracking()
            .Where(course => course.IsActive)
            .OrderBy(course => course.Code)
            .Select(course => new LookupItem(course.Id.ToString(), course.Code + " - " + course.NameEnglish + " / " + course.NameArabic))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LookupItem>> GetActiveSectionsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Sections
            .AsNoTracking()
            .Include(section => section.Course)
            .Where(section => section.IsActive)
            .OrderBy(section => section.Course!.Code)
            .ThenBy(section => section.AcademicYear)
            .ThenBy(section => section.Semester)
            .ThenBy(section => section.SectionNumber)
            .Select(section => new LookupItem(
                section.Id.ToString(),
                section.Course!.Code + "-" + section.SectionNumber + " | " + section.AcademicYear + " | " + section.Semester))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LookupItem>> GetActiveStudentsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Students
            .AsNoTracking()
            .Where(student => student.IsActive)
            .OrderBy(student => student.StudentNumber)
            .Select(student => new LookupItem(student.Id.ToString(), student.StudentNumber + " - " + student.NameEnglish + " / " + student.NameArabic))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LookupItem>> GetActiveInstructorsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Instructors
            .AsNoTracking()
            .Where(instructor => instructor.IsActive)
            .OrderBy(instructor => instructor.EmployeeNumber)
            .Select(instructor => new LookupItem(instructor.Id.ToString(), instructor.EmployeeNumber + " - " + instructor.NameEnglish + " / " + instructor.NameArabic))
            .ToListAsync(cancellationToken);
}
