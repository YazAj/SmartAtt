using AttendAI.Application.Common.Models;

namespace AttendAI.Application.Attendance;

public interface IAttendanceService
{
    Task<StudentAttendancePageDto> GetStudentPageAsync(string userId, CancellationToken cancellationToken = default);

    Task<OperationResult<AttendanceChallengeDto>> IssueChallengeAsync(
        string userId,
        Guid lectureSessionId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AttendanceCheckInResultDto>> CheckInAsync(
        string userId,
        AttendanceCheckInCommand command,
        CancellationToken cancellationToken = default);

    Task<InstructorAttendanceRosterDto?> GetInstructorRosterAsync(
        string userId,
        Guid lectureSessionId,
        CancellationToken cancellationToken = default);
}

public interface ILocationVerificationService
{
    LocationVerificationResult Verify(BrowserLocationSample sample, AttendanceLocationPolicy policy);
}

public interface IAttendanceRateLimiter
{
    bool TryAcquire(string userId, DateTimeOffset nowUtc);
}
