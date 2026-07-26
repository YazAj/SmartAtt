using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class AttendanceRecord : AcademicEntity
{
    private AttendanceRecord()
    {
    }

    public AttendanceRecord(
        Guid lectureSessionId,
        Guid studentId,
        Guid attendanceAttemptId,
        Guid faceVerificationAttemptId,
        AttendanceStatus status,
        DateTimeOffset checkedInAtUtc,
        Guid classroomId,
        int allowedRadiusMeters,
        int maximumAcceptedAccuracyMeters,
        decimal distanceMeters,
        decimal browserAccuracyMeters)
    {
        if (lectureSessionId == Guid.Empty)
        {
            throw new ArgumentException("Lecture session is required.", nameof(lectureSessionId));
        }

        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (attendanceAttemptId == Guid.Empty)
        {
            throw new ArgumentException("Attendance attempt is required.", nameof(attendanceAttemptId));
        }

        if (faceVerificationAttemptId == Guid.Empty)
        {
            throw new ArgumentException("Face verification attempt is required.", nameof(faceVerificationAttemptId));
        }

        if (classroomId == Guid.Empty)
        {
            throw new ArgumentException("Classroom is required.", nameof(classroomId));
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Attendance status is not valid.");
        }

        if (allowedRadiusMeters < 0 || maximumAcceptedAccuracyMeters < 0 || distanceMeters < 0 || browserAccuracyMeters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceMeters), "Attendance location metadata cannot be negative.");
        }

        LectureSessionId = lectureSessionId;
        StudentId = studentId;
        AttendanceAttemptId = attendanceAttemptId;
        FaceVerificationAttemptId = faceVerificationAttemptId;
        Status = status;
        CheckedInAtUtc = checkedInAtUtc;
        ClassroomId = classroomId;
        AllowedRadiusMeters = allowedRadiusMeters;
        MaximumAcceptedAccuracyMeters = maximumAcceptedAccuracyMeters;
        DistanceMeters = distanceMeters;
        BrowserAccuracyMeters = browserAccuracyMeters;
    }

    public Guid LectureSessionId { get; private set; }

    public LectureSession? LectureSession { get; private set; }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid AttendanceAttemptId { get; private set; }

    public AttendanceAttempt? AttendanceAttempt { get; private set; }

    public Guid FaceVerificationAttemptId { get; private set; }

    public FaceVerificationAttempt? FaceVerificationAttempt { get; private set; }

    public AttendanceStatus Status { get; private set; }

    public DateTimeOffset CheckedInAtUtc { get; private set; }

    public Guid ClassroomId { get; private set; }

    public Classroom? Classroom { get; private set; }

    public int AllowedRadiusMeters { get; private set; }

    public int MaximumAcceptedAccuracyMeters { get; private set; }

    public decimal DistanceMeters { get; private set; }

    public decimal BrowserAccuracyMeters { get; private set; }
}
