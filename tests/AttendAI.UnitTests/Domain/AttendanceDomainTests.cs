using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;

namespace AttendAI.UnitTests.Domain;

public sealed class AttendanceDomainTests
{
    [Fact]
    public void Attendance_record_rejects_empty_required_identifiers()
    {
        var exception = Assert.Throws<ArgumentException>(() => new AttendanceRecord(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            AttendanceStatus.Present,
            DateTimeOffset.UtcNow,
            Guid.NewGuid(),
            50,
            75,
            0,
            10));

        Assert.Equal("lectureSessionId", exception.ParamName);
    }

    [Fact]
    public void Attendance_attempt_rejects_success_with_failure_reason()
    {
        var exception = Assert.Throws<ArgumentException>(() => new AttendanceAttempt(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AttendanceAttemptOutcome.Succeeded,
            AttendanceFailureReason.FaceNotMatched,
            LocationVerificationOutcome.Accepted,
            DateTimeOffset.UtcNow,
            new string('a', 64),
            new string('b', 64),
            "Invalid success"));

        Assert.Equal("failureReason", exception.ParamName);
    }

    [Fact]
    public void Attendance_challenge_is_single_use()
    {
        var challenge = new AttendanceChallenge(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new string('c', 64),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(5));

        challenge.Consume(DateTimeOffset.UtcNow, Guid.NewGuid());

        Assert.True(challenge.IsConsumed);
        Assert.Throws<InvalidOperationException>(() => challenge.Consume(DateTimeOffset.UtcNow, Guid.NewGuid()));
    }

    [Fact]
    public void Lecture_session_requires_attendance_coordinate_pair()
    {
        var exception = Assert.Throws<ArgumentException>(() => new LectureSession(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            DateTimeOffset.UtcNow,
            "hash",
            "protected",
            DateTimeOffset.UtcNow.AddMinutes(10),
            10,
            50,
            "user",
            attendanceLatitude: 31.9m));

        Assert.Equal("attendanceLatitude", exception.ParamName);
    }
}
