using System.Security.Cryptography;
using System.Globalization;
using AttendAI.Application.Lectures;
using Microsoft.AspNetCore.DataProtection;

namespace AttendAI.Infrastructure.Lectures;

public sealed class SessionCodeService : ISessionCodeService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private const string HashPrefix = "PBKDF2-SHA256";
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly IDataProtector _protector;

    public SessionCodeService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("AttendAI.LectureSessionCode.v1");
    }

    public string GeneratePlainCode(int length)
    {
        if (length is < 4 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Session code length must be between 4 and 12 characters.");
        }

        Span<char> buffer = stackalloc char[length];
        for (var index = 0; index < buffer.Length; index++)
        {
            buffer[index] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];
        }

        return new string(buffer);
    }

    public string HashCode(string plainCode)
    {
        var normalizedCode = NormalizeCode(plainCode);
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(normalizedCode, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return string.Join(
            ":",
            HashPrefix,
            Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public bool VerifyHash(string plainCode, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(plainCode) || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        var parts = expectedHash.Split(':');
        if (parts.Length != 4 ||
            !string.Equals(parts[0], HashPrefix, StringComparison.Ordinal) ||
            !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expected = Convert.FromBase64String(parts[3]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(NormalizeCode(plainCode), salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public string ProtectCode(string plainCode)
        => _protector.Protect(NormalizeCode(plainCode));

    public string UnprotectCode(string protectedCode)
        => _protector.Unprotect(protectedCode);

    private static string NormalizeCode(string code)
        => (code ?? string.Empty).Trim().ToUpperInvariant();
}
