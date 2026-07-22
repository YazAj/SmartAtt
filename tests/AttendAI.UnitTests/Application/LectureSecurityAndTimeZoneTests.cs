using AttendAI.Application.Lectures;
using AttendAI.Infrastructure.Lectures;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace AttendAI.UnitTests.Application;

public sealed class LectureSecurityAndTimeZoneTests
{
    [Fact]
    public void Session_code_generation_hashing_and_protection_are_safe()
    {
        using var keyDirectory = new TempDirectory();
        var service = new SessionCodeService(DataProtectionProvider.Create(new DirectoryInfo(keyDirectory.Path)));

        var code = service.GeneratePlainCode(6);
        var hash = service.HashCode(code);
        var secondHash = service.HashCode(code);
        var protectedCode = service.ProtectCode(code);

        Assert.Equal(6, code.Length);
        Assert.NotEqual(code, hash);
        Assert.NotEqual(hash, secondHash);
        Assert.NotEqual(code, protectedCode);
        Assert.Equal(code, service.UnprotectCode(protectedCode));
        Assert.True(service.VerifyHash(code, hash));
        Assert.False(service.VerifyHash(code + "X", hash));
    }

    [Fact]
    public void Application_time_zone_converts_between_local_and_utc()
    {
        var service = new ApplicationTimeZoneService(Options.Create(new LectureSchedulingOptions
        {
            TimeZoneId = "UTC",
            WindowsTimeZoneId = "UTC"
        }));

        var utc = service.ConvertLocalToUtc(new DateOnly(2026, 7, 22), new TimeOnly(10, 30));
        var local = service.ConvertUtcToLocal(utc);

        Assert.Equal(TimeSpan.Zero, utc.Offset);
        Assert.Equal(new DateOnly(2026, 7, 22), DateOnly.FromDateTime(local.DateTime));
        Assert.Equal(new TimeOnly(10, 30), TimeOnly.FromDateTime(local.DateTime));
        Assert.True(service.IsWithinStartWindow(utc.AddMinutes(-10), utc, earlyMinutes: 15, lateMinutes: 30));
        Assert.False(service.IsWithinStartWindow(utc.AddMinutes(-20), utc, earlyMinutes: 15, lateMinutes: 30));
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "attendai-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
