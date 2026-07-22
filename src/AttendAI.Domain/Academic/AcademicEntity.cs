using AttendAI.Domain.Common;

namespace AttendAI.Domain.Academic;

public abstract class AcademicEntity : AuditableEntity
{
    public bool IsActive { get; protected set; } = true;

    public byte[] RowVersion { get; protected set; } = [];

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    protected void Touch()
        => UpdatedAtUtc = DateTimeOffset.UtcNow;

    protected static string RequireText(string value, string fieldName, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{fieldName} cannot exceed {maxLength} characters.", fieldName);
        }

        return trimmed;
    }

    protected static string? OptionalText(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", nameof(value));
        }

        return trimmed;
    }

    protected static string NormalizeCode(string value, string fieldName, int maxLength)
        => RequireText(value, fieldName, maxLength).ToUpperInvariant();
}
