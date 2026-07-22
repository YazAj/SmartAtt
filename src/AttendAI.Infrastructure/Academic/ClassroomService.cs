using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class ClassroomService : AcademicServiceBase, IClassroomService
{
    public ClassroomService(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<PagedResult<ClassroomDto>> GetPagedAsync(ClassroomQuery query, CancellationToken cancellationToken = default)
    {
        var classrooms = DbContext.Classrooms.AsNoTracking();
        var search = NormalizeText(query.Search);

        if (search.Length > 0)
        {
            classrooms = classrooms.Where(classroom =>
                classroom.Code.Contains(search) ||
                classroom.BuildingNameEnglish.Contains(search) ||
                classroom.BuildingNameArabic.Contains(search) ||
                classroom.RoomNumber.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            classrooms = classrooms.Where(classroom => classroom.IsActive == query.IsActive.Value);
        }

        classrooms = query.SortBy?.ToLowerInvariant() switch
        {
            "building" => query.SortDescending ? classrooms.OrderByDescending(c => c.BuildingNameEnglish) : classrooms.OrderBy(c => c.BuildingNameEnglish),
            "room" => query.SortDescending ? classrooms.OrderByDescending(c => c.RoomNumber) : classrooms.OrderBy(c => c.RoomNumber),
            "capacity" => query.SortDescending ? classrooms.OrderByDescending(c => c.Capacity) : classrooms.OrderBy(c => c.Capacity),
            "status" => query.SortDescending ? classrooms.OrderByDescending(c => c.IsActive) : classrooms.OrderBy(c => c.IsActive),
            _ => query.SortDescending ? classrooms.OrderByDescending(c => c.Code) : classrooms.OrderBy(c => c.Code)
        };

        var total = await classrooms.CountAsync(cancellationToken);
        var items = await ApplyPaging(classrooms, query).Select(classroom => AcademicProjection.ToDto(classroom)).ToListAsync(cancellationToken);
        return new PagedResult<ClassroomDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<ClassroomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var classroom = await DbContext.Classrooms.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return classroom is null ? null : AcademicProjection.ToDto(classroom);
    }

    public async Task<OperationResult<Guid>> CreateAsync(ClassroomCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(command.Code);
        if (await DbContext.Classrooms.AnyAsync(classroom => classroom.Code == normalizedCode, cancellationToken))
        {
            return DuplicateFailure<Guid>(nameof(command.Code), "ErrorClassroomCodeExists", normalizedCode);
        }

        Classroom classroom;
        try
        {
            classroom = new Classroom(command.Code, command.BuildingNameEnglish, command.BuildingNameArabic, command.RoomNumber, command.Capacity, command.Latitude, command.Longitude);
        }
        catch (ArgumentException exception)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.Classrooms.Add(classroom);
        return await SaveCreatedAsync(classroom.Id, cancellationToken);
    }

    public async Task<OperationResult> UpdateAsync(Guid id, ClassroomCommand command, CancellationToken cancellationToken = default)
    {
        var classroom = await DbContext.Classrooms.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (classroom is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var normalizedCode = NormalizeCode(command.Code);
        if (await DbContext.Classrooms.AnyAsync(item => item.Id != id && item.Code == normalizedCode, cancellationToken))
        {
            return DuplicateFailure(nameof(command.Code), "ErrorClassroomCodeExists", normalizedCode);
        }

        try
        {
            SetOriginalRowVersion(classroom, command.RowVersion);
            classroom.Update(command.Code, command.BuildingNameEnglish, command.BuildingNameArabic, command.RoomNumber, command.Capacity, command.Latitude, command.Longitude);
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
        var classroom = await DbContext.Classrooms.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (classroom is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (isActive)
        {
            classroom.Activate();
        }
        else
        {
            classroom.Deactivate();
        }

        return await SaveChangesAsync(cancellationToken);
    }
}
