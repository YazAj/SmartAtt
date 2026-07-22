using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;

namespace AttendAI.UnitTests.Domain;

public sealed class LectureDomainTests
{
    [Fact]
    public void Lecture_schedule_requires_start_before_end()
    {
        Assert.Throws<ArgumentException>(() => new LectureSchedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DayOfWeek.Sunday,
            new TimeOnly(10, 0),
            new TimeOnly(10, 0),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 12, 31),
            10,
            50));
    }

    [Fact]
    public void Lecture_schedule_rejects_late_threshold_equal_to_duration()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LectureSchedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DayOfWeek.Sunday,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 12, 31),
            60,
            50));
    }

    [Fact]
    public void Lecture_session_end_invalidates_code_and_prevents_reactivation()
    {
        var session = CreateSession();

        session.End("user-1", DateTimeOffset.UtcNow.AddMinutes(30));

        Assert.Equal(LectureSessionStatus.Ended, session.Status);
        Assert.Equal(string.Empty, session.SessionCodeHash);
        Assert.Equal(string.Empty, session.ProtectedSessionCode);
        Assert.Null(session.SessionCodeExpiresAtUtc);
        Assert.Throws<InvalidOperationException>(() => session.RegenerateCode("hash", "protected", DateTimeOffset.UtcNow.AddMinutes(15)));
    }

    [Fact]
    public void Lecture_session_cancel_invalidates_code()
    {
        var session = CreateSession();

        session.Cancel("user-1", DateTimeOffset.UtcNow.AddMinutes(10), "Cancelled safely");

        Assert.Equal(LectureSessionStatus.Cancelled, session.Status);
        Assert.Equal(string.Empty, session.SessionCodeHash);
        Assert.Equal(string.Empty, session.ProtectedSessionCode);
    }

    [Fact]
    public void Lecture_session_expire_is_idempotent()
    {
        var session = CreateSession();

        session.Expire(DateTimeOffset.UtcNow.AddMinutes(70));
        session.Expire(DateTimeOffset.UtcNow.AddMinutes(80));

        Assert.Equal(LectureSessionStatus.Expired, session.Status);
        Assert.Equal(string.Empty, session.SessionCodeHash);
    }

    private static LectureSession CreateSession()
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 7, 22),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow,
            "hash",
            "protected-code",
            DateTimeOffset.UtcNow.AddMinutes(15),
            10,
            50,
            "user-1");
}
