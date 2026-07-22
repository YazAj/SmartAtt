using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class SectionService : AcademicServiceBase, ISectionService
{
    public SectionService(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<PagedResult<SectionDto>> GetPagedAsync(SectionQuery query, CancellationToken cancellationToken = default)
    {
        var sections = DbContext.Sections
            .Include(section => section.Course)
            .Include(section => section.Enrollments)
            .AsNoTracking();

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            sections = sections.Where(section =>
                section.SectionNumber.Contains(search) ||
                section.AcademicYear.Contains(search) ||
                section.Course!.Code.Contains(search) ||
                section.Course.NameEnglish.Contains(search) ||
                section.Course.NameArabic.Contains(search));
        }

        if (query.CourseId.HasValue)
        {
            sections = sections.Where(section => section.CourseId == query.CourseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.AcademicYear))
        {
            sections = sections.Where(section => section.AcademicYear == query.AcademicYear);
        }

        if (query.Semester.HasValue)
        {
            sections = sections.Where(section => section.Semester == query.Semester.Value);
        }

        if (query.IsActive.HasValue)
        {
            sections = sections.Where(section => section.IsActive == query.IsActive.Value);
        }

        sections = query.SortBy?.ToLowerInvariant() switch
        {
            "course" => query.SortDescending ? sections.OrderByDescending(s => s.Course!.Code) : sections.OrderBy(s => s.Course!.Code),
            "year" => query.SortDescending ? sections.OrderByDescending(s => s.AcademicYear) : sections.OrderBy(s => s.AcademicYear),
            "semester" => query.SortDescending ? sections.OrderByDescending(s => s.Semester) : sections.OrderBy(s => s.Semester),
            "capacity" => query.SortDescending ? sections.OrderByDescending(s => s.Capacity) : sections.OrderBy(s => s.Capacity),
            "status" => query.SortDescending ? sections.OrderByDescending(s => s.IsActive) : sections.OrderBy(s => s.IsActive),
            _ => query.SortDescending ? sections.OrderByDescending(s => s.SectionNumber) : sections.OrderBy(s => s.SectionNumber)
        };

        var total = await sections.CountAsync(cancellationToken);
        var items = await ApplyPaging(sections, query).Select(section => AcademicProjection.ToDto(section)).ToListAsync(cancellationToken);
        return new PagedResult<SectionDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<SectionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var section = await DbContext.Sections
            .Include(item => item.Course)
            .Include(item => item.Enrollments)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return section is null ? null : AcademicProjection.ToDto(section);
    }

    public async Task<OperationResult<Guid>> CreateAsync(SectionCommand command, CancellationToken cancellationToken = default)
    {
        var courseIsActive = await DbContext.Courses.AnyAsync(course => course.Id == command.CourseId && course.IsActive, cancellationToken);
        if (!courseIsActive)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorActiveCourseRequired"));
        }

        var sectionNumber = NormalizeCode(command.SectionNumber);
        if (await SectionCompositeExistsAsync(command.CourseId, command.AcademicYear, command.Semester, sectionNumber, null, cancellationToken))
        {
            return OperationResult<Guid>.Failure(OperationErrors.Duplicate(nameof(command.SectionNumber), "ErrorSectionCompositeExists"));
        }

        Section section;
        try
        {
            section = new Section(command.CourseId, command.SectionNumber, command.AcademicYear, command.Semester, command.Capacity);
        }
        catch (ArgumentException exception)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.Sections.Add(section);
        return await SaveCreatedAsync(section.Id, cancellationToken);
    }

    public async Task<OperationResult> UpdateAsync(Guid id, SectionCommand command, CancellationToken cancellationToken = default)
    {
        var section = await DbContext.Sections.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (section is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var courseIsActive = await DbContext.Courses.AnyAsync(course => course.Id == command.CourseId && course.IsActive, cancellationToken);
        if (!courseIsActive)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveCourseRequired"));
        }

        var sectionNumber = NormalizeCode(command.SectionNumber);
        if (await SectionCompositeExistsAsync(command.CourseId, command.AcademicYear, command.Semester, sectionNumber, id, cancellationToken))
        {
            return OperationResult.Failure(OperationErrors.Duplicate(nameof(command.SectionNumber), "ErrorSectionCompositeExists"));
        }

        try
        {
            SetOriginalRowVersion(section, command.RowVersion);
            section.Update(command.CourseId, command.SectionNumber, command.AcademicYear, command.Semester, command.Capacity);
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

    private Task<bool> SectionCompositeExistsAsync(
        Guid courseId,
        string academicYear,
        Domain.Enums.Semester semester,
        string sectionNumber,
        Guid? excludedId,
        CancellationToken cancellationToken)
        => DbContext.Sections.AnyAsync(section =>
            section.CourseId == courseId &&
            section.AcademicYear == academicYear &&
            section.Semester == semester &&
            section.SectionNumber == sectionNumber &&
            (!excludedId.HasValue || section.Id != excludedId.Value),
            cancellationToken);

    private async Task<OperationResult> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var section = await DbContext.Sections.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (section is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (isActive)
        {
            section.Activate();
        }
        else
        {
            section.Deactivate();
        }

        return await SaveChangesAsync(cancellationToken);
    }
}
