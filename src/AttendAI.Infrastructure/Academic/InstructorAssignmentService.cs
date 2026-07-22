using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class InstructorAssignmentService : AcademicServiceBase, IInstructorAssignmentService
{
    public InstructorAssignmentService(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<PagedResult<InstructorAssignmentDto>> GetPagedAsync(InstructorAssignmentQuery query, CancellationToken cancellationToken = default)
    {
        var assignments = DbContext.InstructorAssignments
            .Include(assignment => assignment.Instructor)
            .Include(assignment => assignment.Section)
                .ThenInclude(section => section!.Course)
            .AsNoTracking();

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            assignments = assignments.Where(assignment =>
                assignment.Instructor!.EmployeeNumber.Contains(search) ||
                assignment.Instructor.NameEnglish.Contains(search) ||
                assignment.Instructor.NameArabic.Contains(search) ||
                assignment.Section!.SectionNumber.Contains(search) ||
                assignment.Section.Course!.Code.Contains(search));
        }

        if (query.InstructorId.HasValue)
        {
            assignments = assignments.Where(assignment => assignment.InstructorId == query.InstructorId.Value);
        }

        if (query.SectionId.HasValue)
        {
            assignments = assignments.Where(assignment => assignment.SectionId == query.SectionId.Value);
        }

        if (query.CourseId.HasValue)
        {
            assignments = assignments.Where(assignment => assignment.Section!.CourseId == query.CourseId.Value);
        }

        if (query.IsPrimary.HasValue)
        {
            assignments = assignments.Where(assignment => assignment.IsPrimary == query.IsPrimary.Value);
        }

        if (query.IsActive.HasValue)
        {
            assignments = assignments.Where(assignment => assignment.IsActive == query.IsActive.Value);
        }

        assignments = query.SortBy?.ToLowerInvariant() switch
        {
            "instructor" => query.SortDescending ? assignments.OrderByDescending(a => a.Instructor!.EmployeeNumber) : assignments.OrderBy(a => a.Instructor!.EmployeeNumber),
            "course" => query.SortDescending ? assignments.OrderByDescending(a => a.Section!.Course!.Code) : assignments.OrderBy(a => a.Section!.Course!.Code),
            "primary" => query.SortDescending ? assignments.OrderByDescending(a => a.IsPrimary) : assignments.OrderBy(a => a.IsPrimary),
            "status" => query.SortDescending ? assignments.OrderByDescending(a => a.IsActive) : assignments.OrderBy(a => a.IsActive),
            _ => query.SortDescending ? assignments.OrderByDescending(a => a.AssignedAtUtc) : assignments.OrderBy(a => a.AssignedAtUtc)
        };

        var total = await assignments.CountAsync(cancellationToken);
        var rows = await ApplyPaging(assignments, query).ToListAsync(cancellationToken);
        var items = rows.Select(ToDto).ToList();
        return new PagedResult<InstructorAssignmentDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<InstructorAssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await DbContext.InstructorAssignments
            .Include(item => item.Instructor)
            .Include(item => item.Section)
                .ThenInclude(section => section!.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return assignment is null ? null : ToDto(assignment);
    }

    public async Task<OperationResult<Guid>> CreateAsync(InstructorAssignmentCommand command, CancellationToken cancellationToken = default)
    {
        var instructorIsActive = await DbContext.Instructors.AnyAsync(instructor => instructor.Id == command.InstructorId && instructor.IsActive, cancellationToken);
        if (!instructorIsActive)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorActiveInstructorRequired"));
        }

        var sectionIsActive = await DbContext.Sections.AnyAsync(section => section.Id == command.SectionId && section.IsActive, cancellationToken);
        if (!sectionIsActive)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorActiveSectionRequired"));
        }

        if (await DbContext.InstructorAssignments.AnyAsync(item => item.InstructorId == command.InstructorId && item.SectionId == command.SectionId, cancellationToken))
        {
            return OperationResult<Guid>.Failure(OperationErrors.Duplicate(nameof(command.SectionId), "ErrorDuplicateAssignment"));
        }

        if (command.IsPrimary && await ActivePrimaryExistsAsync(command.SectionId, null, cancellationToken))
        {
            return OperationResult<Guid>.Failure(OperationErrors.Duplicate(nameof(command.IsPrimary), "ErrorPrimaryInstructorExists"));
        }

        var assignment = new InstructorAssignment(command.InstructorId, command.SectionId, command.IsPrimary);
        DbContext.InstructorAssignments.Add(assignment);
        return await SaveCreatedAsync(assignment.Id, cancellationToken);
    }

    public async Task<OperationResult> SetPrimaryAsync(Guid id, InstructorAssignmentPrimaryCommand command, CancellationToken cancellationToken = default)
    {
        var assignment = await DbContext.InstructorAssignments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (assignment is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (command.IsPrimary && await ActivePrimaryExistsAsync(assignment.SectionId, id, cancellationToken))
        {
            return OperationResult.Failure(OperationErrors.Duplicate(nameof(command.IsPrimary), "ErrorPrimaryInstructorExists"));
        }

        SetOriginalRowVersion(assignment, command.RowVersion);
        assignment.SetPrimary(command.IsPrimary);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await DbContext.InstructorAssignments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (assignment is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        assignment.SetPrimary(false);
        assignment.Deactivate();
        return await SaveChangesAsync(cancellationToken);
    }

    private Task<bool> ActivePrimaryExistsAsync(Guid sectionId, Guid? excludedId, CancellationToken cancellationToken)
        => DbContext.InstructorAssignments.AnyAsync(assignment =>
            assignment.SectionId == sectionId &&
            assignment.IsActive &&
            assignment.IsPrimary &&
            (!excludedId.HasValue || assignment.Id != excludedId.Value),
            cancellationToken);

    private static InstructorAssignmentDto ToDto(InstructorAssignment assignment)
        => new(
            assignment.Id,
            assignment.InstructorId,
            assignment.Instructor?.EmployeeNumber ?? string.Empty,
            assignment.Instructor?.NameEnglish ?? string.Empty,
            assignment.Instructor?.NameArabic ?? string.Empty,
            assignment.SectionId,
            $"{assignment.Section?.Course?.Code}-{assignment.Section?.SectionNumber}",
            assignment.Section?.Course?.Code ?? string.Empty,
            assignment.Section?.Course?.NameEnglish ?? string.Empty,
            assignment.Section?.Course?.NameArabic ?? string.Empty,
            assignment.Section?.AcademicYear ?? string.Empty,
            assignment.Section?.Semester ?? Semester.First,
            assignment.IsPrimary,
            assignment.AssignedAtUtc,
            assignment.IsActive,
            assignment.CreatedAtUtc,
            assignment.UpdatedAtUtc,
            RowVersion(assignment.RowVersion));
}
