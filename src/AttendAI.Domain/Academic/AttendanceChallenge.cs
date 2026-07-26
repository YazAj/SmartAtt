namespace AttendAI.Domain.Academic;

public sealed class AttendanceChallenge : AcademicEntity
{
    private AttendanceChallenge()
    {
        TokenHash = string.Empty;
    }

    public AttendanceChallenge(
        Guid studentId,
        Guid lectureSessionId,
        string tokenHash,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (lectureSessionId == Guid.Empty)
        {
            throw new ArgumentException("Lecture session is required.", nameof(lectureSessionId));
        }

        if (expiresAtUtc <= issuedAtUtc)
        {
            throw new ArgumentException("Challenge expiry must be after issue time.", nameof(expiresAtUtc));
        }

        StudentId = studentId;
        LectureSessionId = lectureSessionId;
        TokenHash = RequireText(tokenHash, nameof(tokenHash), 128);
        IssuedAtUtc = issuedAtUtc;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid LectureSessionId { get; private set; }

    public LectureSession? LectureSession { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset IssuedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? ConsumedAtUtc { get; private set; }

    public Guid? ConsumedByAttendanceAttemptId { get; private set; }

    public bool IsConsumed => ConsumedAtUtc.HasValue;

    public bool IsExpired(DateTimeOffset nowUtc) => ExpiresAtUtc <= nowUtc;

    public void Consume(DateTimeOffset consumedAtUtc, Guid attendanceAttemptId)
    {
        if (attendanceAttemptId == Guid.Empty)
        {
            throw new ArgumentException("Attendance attempt is required.", nameof(attendanceAttemptId));
        }

        if (IsConsumed)
        {
            throw new InvalidOperationException("Attendance challenge has already been consumed.");
        }

        ConsumedAtUtc = consumedAtUtc;
        ConsumedByAttendanceAttemptId = attendanceAttemptId;
        Touch();
    }
}
