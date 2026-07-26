using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class LectureSession : AcademicEntity
{
    private readonly List<LectureSessionEvent> _events = [];

    private LectureSession()
    {
        SessionCodeHash = string.Empty;
        ProtectedSessionCode = string.Empty;
        StartedByUserId = string.Empty;
    }

    public LectureSession(
        Guid lectureScheduleId,
        Guid sectionId,
        Guid instructorId,
        Guid classroomId,
        DateOnly sessionDate,
        DateTimeOffset scheduledStartUtc,
        DateTimeOffset scheduledEndUtc,
        DateTimeOffset actualStartUtc,
        string sessionCodeHash,
        string protectedSessionCode,
        DateTimeOffset sessionCodeExpiresAtUtc,
        int lateThresholdMinutes,
        int allowedRadiusMeters,
        string startedByUserId,
        decimal? attendanceLatitude = null,
        decimal? attendanceLongitude = null,
        int maximumAcceptedAccuracyMeters = 75,
        bool attendanceCheckInEnabled = true,
        bool locationVerificationRequired = true,
        bool faceVerificationRequired = true)
    {
        if (lectureScheduleId == Guid.Empty)
        {
            throw new ArgumentException("Lecture schedule is required.", nameof(lectureScheduleId));
        }

        if (sectionId == Guid.Empty)
        {
            throw new ArgumentException("Section is required.", nameof(sectionId));
        }

        if (instructorId == Guid.Empty)
        {
            throw new ArgumentException("Instructor is required.", nameof(instructorId));
        }

        if (classroomId == Guid.Empty)
        {
            throw new ArgumentException("Classroom is required.", nameof(classroomId));
        }

        if (scheduledStartUtc >= scheduledEndUtc)
        {
            throw new ArgumentException("Scheduled start must be before scheduled end.", nameof(scheduledStartUtc));
        }

        LectureScheduleId = lectureScheduleId;
        SectionId = sectionId;
        InstructorId = instructorId;
        ClassroomId = classroomId;
        SessionDate = sessionDate;
        ScheduledStartUtc = scheduledStartUtc;
        ScheduledEndUtc = scheduledEndUtc;
        ActualStartUtc = actualStartUtc;
        Status = LectureSessionStatus.Active;
        SetCode(sessionCodeHash, protectedSessionCode, sessionCodeExpiresAtUtc);
        LateThresholdMinutes = lateThresholdMinutes;
        AllowedRadiusMeters = allowedRadiusMeters;
        StartedByUserId = RequireText(startedByUserId, nameof(startedByUserId), 450);
        SetAttendancePolicy(
            attendanceLatitude,
            attendanceLongitude,
            maximumAcceptedAccuracyMeters,
            attendanceCheckInEnabled,
            locationVerificationRequired,
            faceVerificationRequired);
    }

    public Guid LectureScheduleId { get; private set; }

    public LectureSchedule? LectureSchedule { get; private set; }

    public Guid SectionId { get; private set; }

    public Section? Section { get; private set; }

    public Guid InstructorId { get; private set; }

    public Instructor? Instructor { get; private set; }

    public Guid ClassroomId { get; private set; }

    public Classroom? Classroom { get; private set; }

    public DateOnly SessionDate { get; private set; }

    public DateTimeOffset ScheduledStartUtc { get; private set; }

    public DateTimeOffset ScheduledEndUtc { get; private set; }

    public DateTimeOffset ActualStartUtc { get; private set; }

    public DateTimeOffset? ActualEndUtc { get; private set; }

    public LectureSessionStatus Status { get; private set; }

    public string SessionCodeHash { get; private set; } = string.Empty;

    public string ProtectedSessionCode { get; private set; } = string.Empty;

    public DateTimeOffset? SessionCodeExpiresAtUtc { get; private set; }

    public int SessionCodeVersion { get; private set; } = 1;

    public int LateThresholdMinutes { get; private set; }

    public int AllowedRadiusMeters { get; private set; }

    public decimal? AttendanceLatitude { get; private set; }

    public decimal? AttendanceLongitude { get; private set; }

    public int MaximumAcceptedAccuracyMeters { get; private set; } = 75;

    public bool AttendanceCheckInEnabled { get; private set; } = true;

    public bool LocationVerificationRequired { get; private set; } = true;

    public bool FaceVerificationRequired { get; private set; } = true;

    public string StartedByUserId { get; private set; }

    public string? EndedByUserId { get; private set; }

    public string? CancelledByUserId { get; private set; }

    public string? EndReason { get; private set; }

    public IReadOnlyCollection<LectureSessionEvent> Events => _events;

    public bool IsActiveSession => Status == LectureSessionStatus.Active;

    public void RegenerateCode(string sessionCodeHash, string protectedSessionCode, DateTimeOffset sessionCodeExpiresAtUtc)
    {
        EnsureActive();
        SetCode(sessionCodeHash, protectedSessionCode, sessionCodeExpiresAtUtc);
        SessionCodeVersion++;
        Touch();
    }

    public void End(string performedByUserId, DateTimeOffset endedAtUtc, string? reason = null, bool adminForceEnd = false)
    {
        EnsureActive();
        Status = LectureSessionStatus.Ended;
        ActualEndUtc = endedAtUtc;
        EndedByUserId = RequireText(performedByUserId, nameof(performedByUserId), 450);
        EndReason = OptionalText(reason, 300);
        InvalidateCode();
        Touch();
    }

    public void Cancel(string performedByUserId, DateTimeOffset cancelledAtUtc, string? reason = null)
    {
        EnsureActive();
        Status = LectureSessionStatus.Cancelled;
        ActualEndUtc = cancelledAtUtc;
        CancelledByUserId = RequireText(performedByUserId, nameof(performedByUserId), 450);
        EndReason = OptionalText(reason, 300);
        InvalidateCode();
        Touch();
    }

    public void Expire(DateTimeOffset expiredAtUtc)
    {
        if (Status != LectureSessionStatus.Active)
        {
            return;
        }

        Status = LectureSessionStatus.Expired;
        ActualEndUtc = expiredAtUtc;
        InvalidateCode();
        Touch();
    }

    private void SetCode(string sessionCodeHash, string protectedSessionCode, DateTimeOffset sessionCodeExpiresAtUtc)
    {
        SessionCodeHash = RequireText(sessionCodeHash, nameof(sessionCodeHash), 256);
        ProtectedSessionCode = RequireText(protectedSessionCode, nameof(protectedSessionCode), 2048);
        SessionCodeExpiresAtUtc = sessionCodeExpiresAtUtc;
    }

    private void SetAttendancePolicy(
        decimal? attendanceLatitude,
        decimal? attendanceLongitude,
        int maximumAcceptedAccuracyMeters,
        bool attendanceCheckInEnabled,
        bool locationVerificationRequired,
        bool faceVerificationRequired)
    {
        if (maximumAcceptedAccuracyMeters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAcceptedAccuracyMeters), "Maximum accepted accuracy must be greater than zero.");
        }

        if (attendanceLatitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(attendanceLatitude), "Latitude must be between -90 and 90.");
        }

        if (attendanceLongitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(attendanceLongitude), "Longitude must be between -180 and 180.");
        }

        if (attendanceLatitude.HasValue != attendanceLongitude.HasValue)
        {
            throw new ArgumentException("Attendance latitude and longitude must be provided together.", nameof(attendanceLatitude));
        }

        AttendanceLatitude = attendanceLatitude;
        AttendanceLongitude = attendanceLongitude;
        MaximumAcceptedAccuracyMeters = maximumAcceptedAccuracyMeters;
        AttendanceCheckInEnabled = attendanceCheckInEnabled;
        LocationVerificationRequired = locationVerificationRequired;
        FaceVerificationRequired = faceVerificationRequired;
    }

    private void InvalidateCode()
    {
        SessionCodeHash = string.Empty;
        ProtectedSessionCode = string.Empty;
        SessionCodeExpiresAtUtc = null;
    }

    private void EnsureActive()
    {
        if (Status != LectureSessionStatus.Active)
        {
            throw new InvalidOperationException("Only an active session can change state.");
        }
    }
}
