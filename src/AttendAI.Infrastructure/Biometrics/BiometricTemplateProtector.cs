using AttendAI.Application.Biometrics;
using Microsoft.AspNetCore.DataProtection;

namespace AttendAI.Infrastructure.Biometrics;

public sealed class BiometricTemplateProtector : IBiometricTemplateProtector
{
    private readonly IDataProtector _protector;

    public BiometricTemplateProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("AttendAI.BiometricTemplates.v1");
    }

    public byte[] Protect(byte[] template)
        => _protector.Protect(template);

    public byte[] Unprotect(byte[] protectedTemplate)
        => _protector.Unprotect(protectedTemplate);
}
