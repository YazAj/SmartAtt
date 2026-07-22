using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class CourseService : AcademicServiceBase, ICourseService
{
    public CourseService(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<PagedResult<CourseDto>> GetPagedAsync(CourseQuery query, CancellationToken cancellationToken = default)
    {
        var courses = DbContext.Courses.Include(course => course.Department).AsNoTracking();
        var search = NormalizeText(query.Search);

        if (search.Length > 0)
        {
            courses = courses.Where(course =>
                course.Code.Contains(search) ||
                course.NameEnglish.Contains(search) ||
                course.NameArabic.Contains(search));
        }

        if (query.DepartmentId.HasValue)
        {
            courses = courses.Where(course => course.DepartmentId == query.DepartmentId.Value);
        }

        if (query.IsActive.HasValue)
        {
            courses = courses.Where(course => course.IsActive == query.IsActive.Value);
        }

        courses = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending ? courses.OrderByDescending(c => c.NameEnglish) : courses.OrderBy(c => c.NameEnglish),
            "department" => query.SortDescending ? courses.OrderByDescending(c => c.Department!.NameEnglish) : courses.OrderBy(c => c.Department!.NameEnglish),
            "status" => query.SortDescending ? courses.OrderByDescending(c => c.IsActive) : courses.OrderBy(c => c.IsActive),
            _ => query.SortDescending ? courses.OrderByDescending(c => c.Code) : courses.OrderBy(c => c.Code)
        };

        var total = await courses.CountAsync(cancellationToken);
        var items = await ApplyPaging(courses, query).Select(course => AcademicProjection.ToDto(course)).ToListAsync(cancellationToken);
        return new PagedResult<CourseDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<CourseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var course = await DbContext.Courses.Include(item => item.Department).AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return course is null ? null : AcademicProjection.ToDto(course);
    }

    public async Task<OperationResult<Guid>> CreateAsync(CourseCommand command, CancellationToken cancellationToken = default)
    {
        var departmentIsActive = await DbContext.Departments.AnyAsync(department => department.Id == command.DepartmentId && department.IsActive, cancellationToken);
        if (!departmentIsActive)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorActiveDepartmentRequired"));
        }

        var normalizedCode = NormalizeCode(command.Code);
        if (await DbContext.Courses.AnyAsync(course => course.Code == normalizedCode, cancellationToken))
        {
            return DuplicateFailure<Guid>(nameof(command.Code), "ErrorCourseCodeExists", normalizedCode);
        }

        Course course;
        try
        {
            course = new Course(command.Code, command.NameEnglish, command.NameArabic, command.DepartmentId, command.CreditHours, command.DescriptionEnglish, command.DescriptionArabic);
        }
        catch (ArgumentException exception)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.Courses.Add(course);
        return await SaveCreatedAsync(course.Id, cancellationToken);
    }

    public async Task<OperationResult> UpdateAsync(Guid id, CourseCommand command, CancellationToken cancellationToken = default)
    {
        var course = await DbContext.Courses.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (course is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var departmentIsActive = await DbContext.Departments.AnyAsync(department => department.Id == command.DepartmentId && department.IsActive, cancellationToken);
        if (!departmentIsActive)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveDepartmentRequired"));
        }

        var normalizedCode = NormalizeCode(command.Code);
        if (await DbContext.Courses.AnyAsync(item => item.Id != id && item.Code == normalizedCode, cancellationToken))
        {
            return DuplicateFailure(nameof(command.Code), "ErrorCourseCodeExists", normalizedCode);
        }

        try
        {
            SetOriginalRowVersion(course, command.RowVersion);
            course.Update(command.Code, command.NameEnglish, command.NameArabic, command.DepartmentId, command.CreditHours, command.DescriptionEnglish, command.DescriptionArabic);
        }
        catch (ArgumentException exception)
        {
            return OperationResult.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        return await SaveChangesAsync(cancellationToken);
    }

    public Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(id, true, cancellationToken);

    public Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(id, false, cancellationToken);

    private async Task<OperationResult> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var course = await DbContext.Courses.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (course is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (isActive)
        {
            course.Activate();
        }
        else
        {
            course.Deactivate();
        }

        return await SaveChangesAsync(cancellationToken);
    }
}
