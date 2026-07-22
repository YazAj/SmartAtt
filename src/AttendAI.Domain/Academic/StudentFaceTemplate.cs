using AttendAI.Domain.Exceptions;

namespace AttendAI.Domain.Academic;

public sealed class StudentFaceTemplate : AcademicEntity
{
    private StudentFaceTemplate()
    {
        ProtectedTemplate = [];
        TemplateFingerprint = string.Empty;
        EngineName = string.Empty;
        EngineVersion = string.Empty;
        ModelName = string.Empty;
        ModelVersion = string.Empty;
        TemplateFormatVersion = string.Empty;
    }

    public StudentFaceTemplate(
        Guid studentId,
        Guid biometricConsentId,
        byte[] protectedTemplate,
        string templateFingerprint,
        string engineName,
        string engineVersion,
        string modelName,
        string modelVersion,
        string templateFormatVersion,
        int embeddingDimension,
        decimal qualityScore,
        int captureCount,
        int templateVersion,
        DateTimeOffset enrolledAtUtc)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (biometricConsentId == Guid.Empty)
        {
            throw new ArgumentException("Biometric consent is required.", nameof(biometricConsentId));
        }

        if (protectedTemplate.Length == 0)
        {
            throw new ArgumentException("Protected template is required.", nameof(protectedTemplate));
        }

        if (embeddingDimension <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(embeddingDimension), "Embedding dimension must be greater than zero.");
        }

        if (qualityScore is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(qualityScore), "Quality score must be between zero and one.");
        }

        if (captureCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(captureCount), "Capture count must be greater than zero.");
        }

        if (templateVersion <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(templateVersion), "Template version must be greater than zero.");
        }

        StudentId = studentId;
        BiometricConsentId = biometricConsentId;
        ProtectedTemplate = protectedTemplate.ToArray();
        TemplateFingerprint = RequireText(templateFingerprint, nameof(templateFingerprint), 128);
        EngineName = RequireText(engineName, nameof(engineName), 80);
        EngineVersion = RequireText(engineVersion, nameof(engineVersion), 80);
        ModelName = RequireText(modelName, nameof(modelName), 120);
        ModelVersion = RequireText(modelVersion, nameof(modelVersion), 80);
        TemplateFormatVersion = RequireText(templateFormatVersion, nameof(templateFormatVersion), 80);
        EmbeddingDimension = embeddingDimension;
        QualityScore = qualityScore;
        CaptureCount = captureCount;
        TemplateVersion = templateVersion;
        EnrolledAtUtc = enrolledAtUtc;
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid BiometricConsentId { get; private set; }

    public BiometricConsent? BiometricConsent { get; private set; }

    public byte[] ProtectedTemplate { get; private set; }

    public string TemplateFingerprint { get; private set; }

    public string EngineName { get; private set; }

    public string EngineVersion { get; private set; }

    public string ModelName { get; private set; }

    public string ModelVersion { get; private set; }

    public string TemplateFormatVersion { get; private set; }

    public int EmbeddingDimension { get; private set; }

    public decimal QualityScore { get; private set; }

    public int CaptureCount { get; private set; }

    public int TemplateVersion { get; private set; }

    public DateTimeOffset EnrolledAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public string? RevokedByUserId { get; private set; }

    public string? RevocationReason { get; private set; }

    public bool RequiresReEnrollment { get; private set; }

    public bool Revoke(
        DateTimeOffset revokedAtUtc,
        string revokedByUserId,
        string reason,
        bool requiresReEnrollment)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        RevokedAtUtc = revokedAtUtc;
        RevokedByUserId = RequireText(revokedByUserId, nameof(revokedByUserId), 450);
        RevocationReason = RequireText(reason, nameof(reason), 300);
        RequiresReEnrollment = requiresReEnrollment;
        Touch();
        return true;
    }

    public new void Activate()
        => throw new DomainException("Revoked biometric templates cannot be reactivated.");
}
