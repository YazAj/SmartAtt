using AttendAI.Domain.Academic;
using AttendAI.Domain.Exceptions;

namespace AttendAI.UnitTests.Domain;

public sealed class BiometricDomainTests
{
    [Fact]
    public void BiometricConsent_withdraws_once_and_cannot_be_reactivated()
    {
        var consent = new BiometricConsent(Guid.NewGuid(), "v1", new string('A', 64), DateTimeOffset.UtcNow, "student-user");

        var withdrawn = consent.Withdraw(DateTimeOffset.UtcNow.AddMinutes(1), "student-user");
        var withdrawnAgain = consent.Withdraw(DateTimeOffset.UtcNow.AddMinutes(2), "student-user");

        Assert.True(withdrawn);
        Assert.False(withdrawnAgain);
        Assert.False(consent.IsActive);
        Assert.NotNull(consent.WithdrawnAtUtc);
        Assert.Throws<DomainException>(() => consent.Activate());
    }

    [Fact]
    public void StudentFaceTemplate_stores_protected_template_copy_and_revokes_once()
    {
        var protectedTemplate = new byte[] { 1, 2, 3, 4 };
        var template = new StudentFaceTemplate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            protectedTemplate,
            new string('B', 64),
            "Fake",
            "fake-v1",
            "Fake model",
            "v1",
            "fake-template-v1",
            32,
            0.88m,
            3,
            1,
            DateTimeOffset.UtcNow);

        protectedTemplate[0] = 9;
        var revoked = template.Revoke(DateTimeOffset.UtcNow.AddMinutes(1), "admin-user", "Student requested withdrawal.", requiresReEnrollment: true);
        var revokedAgain = template.Revoke(DateTimeOffset.UtcNow.AddMinutes(2), "admin-user", "Second revoke.", requiresReEnrollment: false);

        Assert.Equal(1, template.ProtectedTemplate[0]);
        Assert.True(revoked);
        Assert.False(revokedAgain);
        Assert.False(template.IsActive);
        Assert.True(template.RequiresReEnrollment);
        Assert.Throws<DomainException>(() => template.Activate());
    }

    [Fact]
    public void StudentFaceTemplate_rejects_invalid_quality_score()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StudentFaceTemplate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            [1, 2, 3],
            new string('C', 64),
            "Fake",
            "fake-v1",
            "Fake model",
            "v1",
            "fake-template-v1",
            32,
            1.1m,
            3,
            1,
            DateTimeOffset.UtcNow));
    }
}
