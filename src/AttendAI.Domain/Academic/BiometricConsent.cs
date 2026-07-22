using AttendAI.Domain.Exceptions;

namespace AttendAI.Domain.Academic;

public sealed class BiometricConsent : AcademicEntity
{
    private BiometricConsent()
    {
        ConsentVersion = string.Empty;
        ConsentTextHash = string.Empty;
        AcceptedByUserId = string.Empty;
    }

    public BiometricConsent(
        Guid studentId,
        string consentVersion,
        string consentTextHash,
        DateTimeOffset acceptedAtUtc,
        string acceptedByUserId)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        StudentId = studentId;
        ConsentVersion = RequireText(consentVersion, nameof(consentVersion), 40);
        ConsentTextHash = RequireText(consentTextHash, nameof(consentTextHash), 128);
        AcceptedAtUtc = acceptedAtUtc;
        AcceptedByUserId = RequireText(acceptedByUserId, nameof(acceptedByUserId), 450);
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public string ConsentVersion { get; private set; }

    public string ConsentTextHash { get; private set; }

    public DateTimeOffset AcceptedAtUtc { get; private set; }

    public DateTimeOffset? WithdrawnAtUtc { get; private set; }

    public string AcceptedByUserId { get; private set; }

    public string? WithdrawnByUserId { get; private set; }

    public bool Withdraw(DateTimeOffset withdrawnAtUtc, string withdrawnByUserId)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        WithdrawnAtUtc = withdrawnAtUtc;
        WithdrawnByUserId = RequireText(withdrawnByUserId, nameof(withdrawnByUserId), 450);
        Touch();
        return true;
    }

    public new void Activate()
        => throw new DomainException("Withdrawn biometric consent cannot be reactivated.");
}
