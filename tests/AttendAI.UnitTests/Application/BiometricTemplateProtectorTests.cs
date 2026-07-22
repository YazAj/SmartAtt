using AttendAI.Infrastructure.Biometrics;
using Microsoft.AspNetCore.DataProtection;

namespace AttendAI.UnitTests.Application;

public sealed class BiometricTemplateProtectorTests
{
    [Fact]
    public void Protect_round_trips_template_without_returning_plain_bytes()
    {
        var provider = DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"))));
        var protector = new BiometricTemplateProtector(provider);
        var plainTemplate = new byte[] { 9, 8, 7, 6, 5 };

        var protectedTemplate = protector.Protect(plainTemplate);
        var unprotectedTemplate = protector.Unprotect(protectedTemplate);

        Assert.NotEqual(plainTemplate, protectedTemplate);
        Assert.Equal(plainTemplate, unprotectedTemplate);
    }
}
