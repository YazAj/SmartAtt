using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public abstract class AcademicServiceBase
{
    protected AcademicServiceBase(ApplicationDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected ApplicationDbContext DbContext { get; }

    protected static string NormalizeCode(string value)
        => (value ?? string.Empty).Trim().ToUpperInvariant();

    protected static string NormalizeText(string? value)
        => value?.Trim() ?? string.Empty;

    public static string RowVersion(byte[]? rowVersion)
        => rowVersion is { Length: > 0 } ? Convert.ToBase64String(rowVersion) : string.Empty;

    protected static byte[] DecodeRowVersion(string? rowVersion)
    {
        if (string.IsNullOrWhiteSpace(rowVersion))
        {
            return [];
        }

        try
        {
            return Convert.FromBase64String(rowVersion);
        }
        catch (FormatException)
        {
            return [];
        }
    }

    protected void SetOriginalRowVersion<TEntity>(TEntity entity, string? rowVersion)
        where TEntity : AcademicEntity
    {
        var original = DecodeRowVersion(rowVersion);
        if (original.Length > 0)
        {
            DbContext.Entry(entity).Property(item => item.RowVersion).OriginalValue = original;
        }
    }

    protected static IQueryable<TEntity> ApplyPaging<TEntity>(IQueryable<TEntity> query, PagedQuery pagedQuery)
        => query.Skip((pagedQuery.PageNumber - 1) * pagedQuery.PageSize).Take(pagedQuery.PageSize);

    protected static OperationResult ValidationFailure(string fieldName, string messageKey, params object[] arguments)
        => OperationResult.Failure(OperationErrors.Validation(fieldName, messageKey, arguments));

    protected static OperationResult<T> ValidationFailure<T>(string fieldName, string messageKey, params object[] arguments)
        => OperationResult<T>.Failure(OperationErrors.Validation(fieldName, messageKey, arguments));

    protected static OperationResult DuplicateFailure(string fieldName, string messageKey, params object[] arguments)
        => OperationResult.Failure(OperationErrors.Duplicate(fieldName, messageKey, arguments));

    protected static OperationResult<T> DuplicateFailure<T>(string fieldName, string messageKey, params object[] arguments)
        => OperationResult<T>.Failure(OperationErrors.Duplicate(fieldName, messageKey, arguments));

    protected async Task<OperationResult> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await DbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return OperationResult.Failure(OperationErrors.Concurrency());
        }
        catch (DbUpdateException)
        {
            return OperationResult.Failure(new OperationError("Database", "ErrorUniqueConstraint"));
        }
    }

    protected async Task<OperationResult<Guid>> SaveCreatedAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await SaveChangesAsync(cancellationToken);
        return result.Succeeded
            ? OperationResult<Guid>.Success(id)
            : OperationResult<Guid>.Failure(result.Errors.ToArray());
    }
}
