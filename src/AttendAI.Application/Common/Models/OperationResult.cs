namespace AttendAI.Application.Common.Models;

public sealed record OperationError(string Code, string MessageKey, string? FieldName = null, object[]? Arguments = null);

public class OperationResult
{
    protected OperationResult(bool succeeded, IReadOnlyList<OperationError> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public IReadOnlyList<OperationError> Errors { get; }

    public static OperationResult Success()
        => new(true, []);

    public static OperationResult Failure(params OperationError[] errors)
        => new(false, errors);
}

public sealed class OperationResult<T> : OperationResult
{
    private OperationResult(bool succeeded, T? value, IReadOnlyList<OperationError> errors)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static OperationResult<T> Success(T value)
        => new(true, value, []);

    public new static OperationResult<T> Failure(params OperationError[] errors)
        => new(false, default, errors);
}

public static class OperationErrors
{
    public static OperationError NotFound(string messageKey = "ErrorRecordNotFound")
        => new("NotFound", messageKey);

    public static OperationError Validation(string fieldName, string messageKey, params object[] arguments)
        => new("Validation", messageKey, fieldName, arguments);

    public static OperationError Duplicate(string fieldName, string messageKey, params object[] arguments)
        => new("Duplicate", messageKey, fieldName, arguments);

    public static OperationError Dependency(string messageKey, params object[] arguments)
        => new("Dependency", messageKey, null, arguments);

    public static OperationError Concurrency()
        => new("Concurrency", "ErrorConcurrency");

    public static OperationError Identity(string messageKey, params object[] arguments)
        => new("Identity", messageKey, null, arguments);
}
