using AttendAI.Application.Common.Models;

namespace AttendAI.Application.Lectures;

public interface ILectureScheduleService
{
    Task<PagedResult<LectureScheduleDto>> GetPagedAsync(LectureScheduleQuery query, CancellationToken cancellationToken = default);

    Task<LectureScheduleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(LectureScheduleCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateAsync(Guid id, LectureScheduleCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ScheduleConflictResult> CheckConflictAsync(Guid? excludedScheduleId, LectureScheduleCommand command, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LectureScheduleDto>> GetClassroomTimetableAsync(Guid classroomId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LectureScheduleDto>> GetSectionTimetableAsync(Guid sectionId, CancellationToken cancellationToken = default);
}

public interface ILectureSessionService
{
    Task<PagedResult<LectureSessionDto>> GetPagedAsync(LectureSessionQuery query, CancellationToken cancellationToken = default);

    Task<LectureSessionDto?> GetByIdAsync(Guid id, bool includeSensitiveCodeState = false, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LectureSessionEventDto>> GetEventsAsync(Guid lectureSessionId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<InstructorScheduleItemDto>> GetInstructorScheduleAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentScheduleItemDto>> GetStudentScheduleAsync(string userId, CancellationToken cancellationToken = default);

    Task<LectureSessionDto?> GetStudentActiveSessionAsync(string userId, Guid lectureSessionId, CancellationToken cancellationToken = default);

    Task<OperationResult<LectureSessionStartResult>> StartAsync(string userId, StartLectureSessionCommand command, bool adminOverride = false, CancellationToken cancellationToken = default);

    Task<OperationResult<SessionCodeResult>> RegenerateCodeAsync(string userId, Guid lectureSessionId, bool adminOverride = false, CancellationToken cancellationToken = default);

    Task<OperationResult> EndAsync(string userId, Guid lectureSessionId, EndLectureSessionCommand command, bool adminForceEnd = false, CancellationToken cancellationToken = default);

    Task<OperationResult> CancelAsync(string userId, Guid lectureSessionId, EndLectureSessionCommand command, CancellationToken cancellationToken = default);

    Task<int> ExpireStaleSessionsAsync(CancellationToken cancellationToken = default);
}

public interface ILectureLookupService
{
    Task<IReadOnlyList<LookupItem>> GetActiveClassroomsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem>> GetLectureSchedulesAsync(CancellationToken cancellationToken = default);
}

public interface ISessionCodeService
{
    string GeneratePlainCode(int length);

    string HashCode(string plainCode);

    bool VerifyHash(string plainCode, string expectedHash);

    string ProtectCode(string plainCode);

    string UnprotectCode(string protectedCode);
}

public interface IApplicationTimeZoneService
{
    TimeZoneInfo ApplicationTimeZone { get; }

    DateTimeOffset ConvertUtcToLocal(DateTimeOffset utc);

    DateTimeOffset ConvertLocalToUtc(DateOnly date, TimeOnly time);

    DateOnly GetLocalDate(DateTimeOffset utc);

    bool IsWithinStartWindow(DateTimeOffset nowUtc, DateTimeOffset scheduledStartUtc, int earlyMinutes, int lateMinutes);
}
