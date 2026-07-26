using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Attendance;

public sealed record StudentAttendancePageDto(
    FaceEngineDiagnosticsDto Diagnostics,
    IReadOnlyList<AttendanceSessionDto> ActiveSessions,
    IReadOnlyList<StudentAttendanceAttemptDto> RecentAttempts);

public sealed record AttendanceSessionDto(
    Guid LectureSessionId,
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string SectionNumber,
    string InstructorNameEnglish,
    string InstructorNameArabic,
    string ClassroomCode,
    DateTimeOffset ScheduledStartUtc,
    DateTimeOffset ScheduledEndUtc,
    DateTimeOffset ScheduledStartLocal,
    DateTimeOffset ScheduledEndLocal,
    DateTimeOffset ActualStartUtc,
    DateTimeOffset ActualStartLocal,
    int LateThresholdMinutes,
    int AllowedRadiusMeters,
    int MaximumAcceptedAccuracyMeters,
    bool LocationPolicyAvailable,
    bool AlreadyCheckedIn,
    bool CanCheckIn,
    string MessageKey);

public sealed record AttendanceChallengeDto(
    AttendanceSessionDto Session,
    string ChallengeToken,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset ExpiresAtLocal,
    int MaximumCaptureBytes,
    string AllowedContentTypes,
    int MaximumAcceptedAccuracyMeters);

public sealed record AttendanceCheckInResultDto(
    Guid AttendanceAttemptId,
    Guid? AttendanceRecordId,
    Guid? FaceVerificationAttemptId,
    bool Succeeded,
    bool AlreadyRecorded,
    AttendanceStatus? Status,
    AttendanceFailureReason FailureReason,
    LocationVerificationOutcome LocationOutcome,
    string MessageKey,
    DateTimeOffset AttemptedAtUtc,
    DateTimeOffset? CheckedInAtUtc,
    decimal? DistanceMeters,
    decimal? BrowserAccuracyMeters,
    decimal? FaceScore,
    decimal? FaceThreshold,
    FaceVerificationDecision FaceDecision,
    bool IsDemoMode);

public sealed record StudentAttendanceAttemptDto(
    Guid Id,
    Guid LectureSessionId,
    string CourseCode,
    string SectionNumber,
    string ClassroomCode,
    AttendanceAttemptOutcome Outcome,
    AttendanceFailureReason FailureReason,
    LocationVerificationOutcome LocationOutcome,
    DateTimeOffset AttemptedAtUtc,
    DateTimeOffset AttemptedAtLocal,
    decimal? DistanceMeters,
    decimal? BrowserAccuracyMeters,
    Guid? AttendanceRecordId,
    Guid? FaceVerificationAttemptId);

public sealed record InstructorAttendanceRosterDto(
    Guid LectureSessionId,
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string SectionNumber,
    string ClassroomCode,
    DateTimeOffset ScheduledStartLocal,
    DateTimeOffset ScheduledEndLocal,
    int EnrolledCount,
    int PresentCount,
    int LateCount,
    int MissingCount,
    IReadOnlyList<InstructorAttendanceRosterRowDto> Rows);

public sealed record InstructorAttendanceRosterRowDto(
    Guid StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    string Email,
    AttendanceStatus? Status,
    DateTimeOffset? CheckedInAtUtc,
    DateTimeOffset? CheckedInAtLocal,
    decimal? DistanceMeters,
    decimal? BrowserAccuracyMeters,
    FaceVerificationDecision? FaceDecision,
    AttendanceAttemptOutcome? LastAttemptOutcome,
    AttendanceFailureReason? LastFailureReason,
    DateTimeOffset? LastAttemptAtLocal);

public sealed record LocationVerificationResult(
    LocationVerificationOutcome Outcome,
    AttendanceFailureReason FailureReason,
    decimal? DistanceMeters,
    string MessageKey)
{
    public bool Accepted => Outcome == LocationVerificationOutcome.Accepted;
}
