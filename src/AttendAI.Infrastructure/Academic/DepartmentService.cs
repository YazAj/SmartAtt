using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class DepartmentService : AcademicServiceBase, IDepartmentService
{
    public DepartmentService(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<PagedResult<DepartmentDto>> GetPagedAsync(DepartmentQuery query, CancellationToken cancellationToken = default)
    {
        var departments = DbContext.Departments.AsNoTracking();
        var search = NormalizeText(query.Search);

        if (search.Length > 0)
        {
            departments = departments.Where(department =>
                department.Code.Contains(search) ||
                department.NameEnglish.Contains(search) ||
                department.NameArabic.Contains(search));
        }

        if (query.IsActive.HasValue)
        {
            departments = departments.Where(department => department.IsActive == query.IsActive.Value);
        }

        departments = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending ? departments.OrderByDescending(d => d.NameEnglish) : departments.OrderBy(d => d.NameEnglish),
            "status" => query.SortDescending ? departments.OrderByDescending(d => d.IsActive) : departments.OrderBy(d => d.IsActive),
            _ => query.SortDescending ? departments.OrderByDescending(d => d.Code) : departments.OrderBy(d => d.Code)
        };

        var total = await departments.CountAsync(cancellationToken);
        var items = await ApplyPaging(departments, query)
            .Select(department => AcademicProjection.ToDto(department))
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await DbContext.Departments.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return department is null ? null : AcademicProjection.ToDto(department);
    }

    public async Task<OperationResult<Guid>> CreateAsync(DepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(command.Code);
        if (await DbContext.Departments.AnyAsync(department => department.Code == normalizedCode, cancellationToken))
        {
            return DuplicateFailure<Guid>(nameof(command.Code), "ErrorDepartmentCodeExists", normalizedCode);
        }

        Department department;
        try
        {
            department = new Department(command.Code, command.NameEnglish, command.NameArabic, command.DescriptionEnglish, command.DescriptionArabic);
        }
        catch (ArgumentException exception)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.Departments.Add(department);
        return await SaveCreatedAsync(department.Id, cancellationToken);
    }

    public async Task<OperationResult> UpdateAsync(Guid id, DepartmentCommand command, CancellationToken cancellationToken = default)
    {
        var department = await DbContext.Departments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (department is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var normalizedCode = NormalizeCode(command.Code);
        if (await DbContext.Departments.AnyAsync(item => item.Id != id && item.Code == normalizedCode, cancellationToken))
        {
            return DuplicateFailure(nameof(command.Code), "ErrorDepartmentCodeExists", normalizedCode);
        }

        try
        {
            SetOriginalRowVersion(department, command.RowVersion);
            department.Update(command.Code, command.NameEnglish, command.NameArabic, command.DescriptionEnglish, command.DescriptionArabic);
        }
        catch (ArgumentException exception)
        {
            return OperationResult.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await DbContext.Departments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (department is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        department.Activate();
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await DbContext.Departments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (department is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        department.Deactivate();
        return await SaveChangesAsync(cancellationToken);
    }
}
