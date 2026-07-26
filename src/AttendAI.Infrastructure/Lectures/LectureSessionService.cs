using System.Data;
using AttendAI.Application.Attendance;
using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.Common.Models;
using AttendAI.Application.Lectures;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Lectures;

public sealed class LectureSessionService : AcademicServiceBase, ILectureSessionService
{
    private readonly IApplicationTimeZoneService _timeZoneService;
    private readonly ISessionCodeService _sessionCodeService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly LectureSchedulingOptions _options;
    private readonly AttendanceOptions _attendanceOptions;

    public LectureSessionService(
        ApplicationDbContext dbContext,
        IApplicationTimeZoneService timeZoneService,
        ISessionCodeService sessionCodeService,
        IDateTimeProvider dateTimeProvider,
        IOptions<LectureSchedulingOptions> options,
        IOptions<AttendanceOptions> attendanceOptions)
        : base(dbContext)
    {
        _timeZoneService = timeZoneService;
        _sessionCodeService = sessionCodeService;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _attendanceOptions = attendanceOptions.Value;
    }

    public async Task<PagedResult<LectureSessionDto>> GetPagedAsync(LectureSessionQuery query, CancellationToken cancellationToken = default)
    {
        var sessions = BaseSessionQuery().AsNoTracking();

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            sessions = sessions.Where(session =>
                session.Section!.SectionNumber.Contains(search) ||
                session.Section.Course!.Code.Contains(search) ||
                session.Section.Course.NameEnglish.Contains(search) ||
                session.Section.Course.NameArabic.Contains(search) ||
                session.Instructor!.NameEnglish.Contains(search) ||
                session.Instructor.NameArabic.Contains(search) ||
                session.Classroom!.Code.Contains(search));
        }

        if (query.SectionId.HasValue)
        {
            sessions = sessions.Where(session => session.SectionId == query.SectionId.Value);
        }

        if (query.InstructorId.HasValue)
        {
            sessions = sessions.Where(session => session.InstructorId == query.InstructorId.Value);
        }

        if (query.ClassroomId.HasValue)
        {
            sessions = sessions.Where(session => session.ClassroomId == query.ClassroomId.Value);
        }

        if (query.Status.HasValue)
        {
            sessions = sessions.Where(session => session.Status == query.Status.Value);
        }

        sessions = query.SortBy?.ToLowerInvariant() switch
        {
            "course" => query.SortDescending ? sessions.OrderByDescending(s => s.Section!.Course!.Code) : sessions.OrderBy(s => s.Section!.Course!.Code),
            "status" => query.SortDescending ? sessions.OrderByDescending(s => s.Status) : sessions.OrderBy(s => s.Status),
            "instructor" => query.SortDescending ? sessions.OrderByDescending(s => s.Instructor!.NameEnglish) : sessions.OrderBy(s => s.Instructor!.NameEnglish),
            "classroom" => query.SortDescending ? sessions.OrderByDescending(s => s.Classroom!.Code) : sessions.OrderBy(s => s.Classroom!.Code),
            _ => query.SortDescending
                ? sessions.OrderBy(s => s.SessionDate).ThenBy(s => s.Id)
                : sessions.OrderByDescending(s => s.SessionDate).ThenByDescending(s => s.Id)
        };

        var total = await sessions.CountAsync(cancellationToken);
        var rows = await ApplyPaging(sessions, query).ToListAsync(cancellationToken);
        return new PagedResult<LectureSessionDto>(
            rows.Select(row => LectureProjection.ToDto(row, _timeZoneService, includeSensitiveCodeState: true)).ToList(),
            query.PageNumber,
            query.PageSize,
            total);
    }

    public async Task<LectureSessionDto?> GetByIdAsync(Guid id, bool includeSensitiveCodeState = false, CancellationToken cancellationToken = default)
    {
        var session = await BaseSessionQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return session is null ? null : LectureProjection.ToDto(session, _timeZoneService, includeSensitiveCodeState);
    }

    public async Task<IReadOnlyList<LectureSessionEventDto>> GetEventsAsync(Guid lectureSessionId, CancellationToken cancellationToken = default)
    {
        var events = await DbContext.LectureSessionEvents
            .AsNoTracking()
            .Where(@event => @event.LectureSessionId == lectureSessionId)
            .OrderBy(@event => @event.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        return events.Select(@event => LectureProjection.ToDto(@event, _timeZoneService)).ToList();
    }

    public async Task<IReadOnlyList<InstructorScheduleItemDto>> GetInstructorScheduleAsync(string userId, CancellationToken cancellationToken = default)
    {
        var instructor = await DbContext.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ApplicationUserId == userId && item.IsActive, cancellationToken);

        if (instructor is null)
        {
            return [];
        }

        var schedules = await DbContext.LectureSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Section)
                .ThenInclude(section => section!.Course)
            .Include(schedule => schedule.Classroom)
            .Where(schedule => schedule.InstructorId == instructor.Id && schedule.IsActive)
            .OrderBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);

        var scheduleIds = schedules.Select(schedule => schedule.Id).ToList();
        var activeSessions = await DbContext.LectureSessions
            .AsNoTracking()
            .Where(session => scheduleIds.Contains(session.LectureScheduleId) && session.Status == LectureSessionStatus.Active)
            .Select(session => new { session.Id, session.LectureScheduleId, session.Status })
            .ToListAsync(cancellationToken);

        var nowUtc = _dateTimeProvider.UtcNow;
        var localDate = _timeZoneService.GetLocalDate(nowUtc);

        return schedules
            .Select(schedule =>
            {
                var activeSession = activeSessions.FirstOrDefault(session => session.LectureScheduleId == schedule.Id);
                var scheduledStartUtc = _timeZoneService.ConvertLocalToUtc(localDate, schedule.StartTime);
                var canStart = activeSession is null &&
                    schedule.IsEffectiveOn(localDate) &&
                    _timeZoneService.IsWithinStartWindow(nowUtc, scheduledStartUtc, _options.EarlyStartWindowMinutes, _options.LateStartWindowMinutes);

                return new InstructorScheduleItemDto(
                    schedule.Id,
                    activeSession?.Id,
                    schedule.Section?.Course?.Code ?? string.Empty,
                    schedule.Section?.Course?.NameEnglish ?? string.Empty,
                    schedule.Section?.Course?.NameArabic ?? string.Empty,
                    schedule.Section?.SectionNumber ?? string.Empty,
                    schedule.Classroom?.Code ?? string.Empty,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.EffectiveFrom,
                    schedule.EffectiveTo,
                    canStart,
                    activeSession?.Status);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<StudentScheduleItemDto>> GetStudentScheduleAsync(string userId, CancellationToken cancellationToken = default)
    {
        var student = await DbContext.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ApplicationUserId == userId && item.IsActive, cancellationToken);

        if (student is null)
        {
            return [];
        }

        var sectionIds = await DbContext.StudentEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == student.Id && enrollment.IsActive)
            .Select(enrollment => enrollment.SectionId)
            .ToListAsync(cancellationToken);

        var schedules = await DbContext.LectureSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Section)
                .ThenInclude(section => section!.Course)
            .Include(schedule => schedule.Classroom)
            .Where(schedule => sectionIds.Contains(schedule.SectionId) && schedule.IsActive)
            .OrderBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);

        var scheduleIds = schedules.Select(schedule => schedule.Id).ToList();
        var activeSessions = await DbContext.LectureSessions
            .AsNoTracking()
            .Where(session => scheduleIds.Contains(session.LectureScheduleId) && session.Status == LectureSessionStatus.Active)
            .Select(session => new { session.Id, session.LectureScheduleId })
            .ToListAsync(cancellationToken);

        return schedules
            .Select(schedule =>
            {
                var activeSession = activeSessions.FirstOrDefault(session => session.LectureScheduleId == schedule.Id);
                return new StudentScheduleItemDto(
                    schedule.Id,
                    activeSession?.Id,
                    schedule.Section?.Course?.Code ?? string.Empty,
                    schedule.Section?.Course?.NameEnglish ?? string.Empty,
                    schedule.Section?.Course?.NameArabic ?? string.Empty,
                    schedule.Section?.SectionNumber ?? string.Empty,
                    schedule.Classroom?.Code ?? string.Empty,
                    schedule.DayOfWeek,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.EffectiveFrom,
                    schedule.EffectiveTo,
                    activeSession is not null);
            })
            .ToList();
    }

    public async Task<LectureSessionDto?> GetStudentActiveSessionAsync(string userId, Guid lectureSessionId, CancellationToken cancellationToken = default)
    {
        var studentId = await DbContext.Students
            .AsNoTracking()
            .Where(student => student.ApplicationUserId == userId && student.IsActive)
            .Select(student => student.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (studentId == Guid.Empty)
        {
            return null;
        }

        var session = await BaseSessionQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == lectureSessionId && item.Status == LectureSessionStatus.Active, cancellationToken);

        if (session is null)
        {
            return null;
        }

        var isEnrolled = await DbContext.StudentEnrollments.AnyAsync(enrollment =>
            enrollment.StudentId == studentId &&
            enrollment.SectionId == session.SectionId &&
            enrollment.IsActive,
            cancellationToken);

        return isEnrolled ? LectureProjection.ToDto(session, _timeZoneService, includeSensitiveCodeState: false) : null;
    }

    public async Task<OperationResult<LectureSessionStartResult>> StartAsync(
        string userId,
        StartLectureSessionCommand command,
        bool adminOverride = false,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var schedule = await BaseScheduleQuery()
            .FirstOrDefaultAsync(item => item.Id == command.LectureScheduleId, cancellationToken);

        if (schedule is null)
        {
            return OperationResult<LectureSessionStartResult>.Failure(OperationErrors.NotFound("ErrorLectureScheduleNotFound"));
        }

        var validationError = await ValidateStartAsync(schedule, userId, command.SessionDate, adminOverride, cancellationToken);
        if (validationError.Error is not null)
        {
            return OperationResult<LectureSessionStartResult>.Failure(validationError.Error);
        }

        var code = CreateSessionCode(_dateTimeProvider.UtcNow);
        var session = new LectureSession(
            schedule.Id,
            schedule.SectionId,
            schedule.InstructorId,
            schedule.ClassroomId,
            validationError.SessionDate,
            validationError.ScheduledStartUtc,
            validationError.ScheduledEndUtc,
            _dateTimeProvider.UtcNow,
            code.Hash,
            code.Protected,
            code.ExpiresAtUtc,
            schedule.DefaultLateThresholdMinutes,
            schedule.DefaultAllowedRadiusMeters,
            userId,
            schedule.Classroom?.Latitude,
            schedule.Classroom?.Longitude,
            Math.Max(1, _attendanceOptions.MaximumAcceptedAccuracyMeters),
            _attendanceOptions.AttendanceEnabled,
            _attendanceOptions.RequireBrowserLocation,
            faceVerificationRequired: true);

        DbContext.LectureSessions.Add(session);
        AddEvent(session.Id, LectureSessionEventType.Started, userId, null, LectureSessionStatus.Active, adminOverride ? "Admin override start." : "Session started.");
        AddEvent(session.Id, LectureSessionEventType.CodeGenerated, userId, LectureSessionStatus.Active, LectureSessionStatus.Active, "Initial session code generated.");

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return OperationResult<LectureSessionStartResult>.Failure(result.Errors.ToArray());
        }

        await transaction.CommitAsync(cancellationToken);
        return OperationResult<LectureSessionStartResult>.Success(new LectureSessionStartResult(
            session.Id,
            code.Plain,
            code.ExpiresAtUtc,
            _timeZoneService.ConvertUtcToLocal(code.ExpiresAtUtc)));
    }

    public async Task<OperationResult<SessionCodeResult>> RegenerateCodeAsync(
        string userId,
        Guid lectureSessionId,
        bool adminOverride = false,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var session = await BaseSessionQuery().FirstOrDefaultAsync(item => item.Id == lectureSessionId, cancellationToken);
        if (session is null)
        {
            return OperationResult<SessionCodeResult>.Failure(OperationErrors.NotFound("ErrorLectureSessionNotFound"));
        }

        var authorizationError = await ValidateSessionOwnershipAsync(session, userId, adminOverride, cancellationToken);
        if (authorizationError is not null)
        {
            return OperationResult<SessionCodeResult>.Failure(authorizationError);
        }

        if (session.Status != LectureSessionStatus.Active)
        {
            return OperationResult<SessionCodeResult>.Failure(OperationErrors.Dependency("ErrorLectureSessionMustBeActive"));
        }

        var code = CreateSessionCode(_dateTimeProvider.UtcNow);
        session.RegenerateCode(code.Hash, code.Protected, code.ExpiresAtUtc);
        AddEvent(session.Id, LectureSessionEventType.CodeRegenerated, userId, LectureSessionStatus.Active, LectureSessionStatus.Active, "Session code regenerated.");

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return OperationResult<SessionCodeResult>.Failure(result.Errors.ToArray());
        }

        await transaction.CommitAsync(cancellationToken);
        return OperationResult<SessionCodeResult>.Success(new SessionCodeResult(
            code.Plain,
            code.ExpiresAtUtc,
            _timeZoneService.ConvertUtcToLocal(code.ExpiresAtUtc)));
    }

    public async Task<OperationResult> EndAsync(
        string userId,
        Guid lectureSessionId,
        EndLectureSessionCommand command,
        bool adminForceEnd = false,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var session = await BaseSessionQuery().FirstOrDefaultAsync(item => item.Id == lectureSessionId, cancellationToken);
        if (session is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound("ErrorLectureSessionNotFound"));
        }

        var authorizationError = await ValidateSessionOwnershipAsync(session, userId, adminForceEnd, cancellationToken);
        if (authorizationError is not null)
        {
            return OperationResult.Failure(authorizationError);
        }

        SetOriginalRowVersion(session, command.RowVersion);
        try
        {
            session.End(userId, _dateTimeProvider.UtcNow, command.Reason, adminForceEnd);
        }
        catch (InvalidOperationException)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorLectureSessionMustBeActive"));
        }

        AddEvent(
            session.Id,
            adminForceEnd ? LectureSessionEventType.AdminForceEnded : LectureSessionEventType.Ended,
            userId,
            LectureSessionStatus.Active,
            LectureSessionStatus.Ended,
            adminForceEnd ? "Session force-ended by admin." : "Session ended.");

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<OperationResult> CancelAsync(string userId, Guid lectureSessionId, EndLectureSessionCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var session = await BaseSessionQuery().FirstOrDefaultAsync(item => item.Id == lectureSessionId, cancellationToken);
        if (session is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound("ErrorLectureSessionNotFound"));
        }

        var authorizationError = await ValidateSessionOwnershipAsync(session, userId, adminOverride: false, cancellationToken);
        if (authorizationError is not null)
        {
            return OperationResult.Failure(authorizationError);
        }

        SetOriginalRowVersion(session, command.RowVersion);
        try
        {
            session.Cancel(userId, _dateTimeProvider.UtcNow, command.Reason);
        }
        catch (InvalidOperationException)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorLectureSessionMustBeActive"));
        }

        AddEvent(session.Id, LectureSessionEventType.Cancelled, userId, LectureSessionStatus.Active, LectureSessionStatus.Cancelled, "Session cancelled.");
        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<int> ExpireStaleSessionsAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var nowUtc = _dateTimeProvider.UtcNow;
        var maximumStartUtc = nowUtc.AddMinutes(-_options.MaximumSessionDurationMinutes);
        var sessions = await DbContext.LectureSessions
            .Where(session =>
                session.Status == LectureSessionStatus.Active &&
                (session.ScheduledEndUtc <= nowUtc ||
                 session.ActualStartUtc <= maximumStartUtc))
            .Take(100)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.Expire(nowUtc);
            AddEvent(session.Id, LectureSessionEventType.Expired, "system", LectureSessionStatus.Active, LectureSessionStatus.Expired, "Session expired automatically.");
        }

        if (sessions.Count == 0)
        {
            return 0;
        }

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return 0;
        }

        await transaction.CommitAsync(cancellationToken);
        return sessions.Count;
    }

    private IQueryable<LectureSession> BaseSessionQuery()
        => DbContext.LectureSessions
            .Include(session => session.Section)
                .ThenInclude(section => section!.Course)
            .Include(session => session.Instructor)
            .Include(session => session.Classroom);

    private IQueryable<LectureSchedule> BaseScheduleQuery()
        => DbContext.LectureSchedules
            .Include(schedule => schedule.Section)
                .ThenInclude(section => section!.Course)
            .Include(schedule => schedule.Instructor)
            .Include(schedule => schedule.Classroom);

    private async Task<StartValidation> ValidateStartAsync(
        LectureSchedule schedule,
        string userId,
        DateOnly? requestedSessionDate,
        bool adminOverride,
        CancellationToken cancellationToken)
    {
        if (!schedule.IsActive ||
            schedule.Section is null ||
            !schedule.Section.IsActive ||
            schedule.Section.Course is null ||
            !schedule.Section.Course.IsActive ||
            schedule.Instructor is null ||
            !schedule.Instructor.IsActive ||
            schedule.Classroom is null ||
            !schedule.Classroom.IsActive)
        {
            return StartValidation.Failure(OperationErrors.Dependency("ErrorLectureScheduleInactive"));
        }

        var instructorUserIsActive = await DbContext.Users.AnyAsync(user =>
            user.Id == schedule.Instructor.ApplicationUserId &&
            user.IsActive &&
            !user.IsDisabled,
            cancellationToken);

        if (!instructorUserIsActive)
        {
            return StartValidation.Failure(OperationErrors.Dependency("ErrorActiveInstructorAccountRequired"));
        }

        if (!adminOverride && !string.Equals(schedule.Instructor.ApplicationUserId, userId, StringComparison.Ordinal))
        {
            return StartValidation.Failure(OperationErrors.Dependency("ErrorLectureInstructorOwnership"));
        }

        var hasAssignment = await DbContext.InstructorAssignments.AnyAsync(assignment =>
            assignment.InstructorId == schedule.InstructorId &&
            assignment.SectionId == schedule.SectionId &&
            assignment.IsActive,
            cancellationToken);

        if (!hasAssignment)
        {
            return StartValidation.Failure(OperationErrors.Dependency("ErrorInstructorAssignmentRequired"));
        }

        var sessionDate = requestedSessionDate ?? _timeZoneService.GetLocalDate(_dateTimeProvider.UtcNow);
        if (!schedule.IsEffectiveOn(sessionDate))
        {
            return StartValidation.Failure(OperationErrors.Validation(nameof(StartLectureSessionCommand.SessionDate), "ErrorLectureOccurrenceDate"));
        }

        var scheduledStartUtc = _timeZoneService.ConvertLocalToUtc(sessionDate, schedule.StartTime);
        var scheduledEndUtc = _timeZoneService.ConvertLocalToUtc(sessionDate, schedule.EndTime);
        if ((scheduledEndUtc - scheduledStartUtc).TotalMinutes > _options.MaximumSessionDurationMinutes)
        {
            return StartValidation.Failure(OperationErrors.Validation(nameof(schedule.EndTime), "ErrorLectureSessionDurationLimit"));
        }

        if (!adminOverride && !_timeZoneService.IsWithinStartWindow(
            _dateTimeProvider.UtcNow,
            scheduledStartUtc,
            _options.EarlyStartWindowMinutes,
            _options.LateStartWindowMinutes))
        {
            return StartValidation.Failure(OperationErrors.Dependency("ErrorLectureStartWindow"));
        }

        var duplicateOccurrence = await DbContext.LectureSessions.AnyAsync(session =>
            session.LectureScheduleId == schedule.Id &&
            session.SessionDate == sessionDate,
            cancellationToken);

        if (duplicateOccurrence)
        {
            return StartValidation.Failure(OperationErrors.Duplicate(nameof(StartLectureSessionCommand.LectureScheduleId), "ErrorLectureOccurrenceExists"));
        }

        var activeResourceConflict = await DbContext.LectureSessions.AnyAsync(session =>
            session.Status == LectureSessionStatus.Active &&
            (session.InstructorId == schedule.InstructorId ||
             session.SectionId == schedule.SectionId ||
             session.ClassroomId == schedule.ClassroomId),
            cancellationToken);

        if (activeResourceConflict)
        {
            return StartValidation.Failure(OperationErrors.Dependency("ErrorLectureActiveResourceConflict"));
        }

        return new StartValidation(null, sessionDate, scheduledStartUtc, scheduledEndUtc);
    }

    private async Task<OperationError?> ValidateSessionOwnershipAsync(LectureSession session, string userId, bool adminOverride, CancellationToken cancellationToken)
    {
        if (adminOverride)
        {
            return null;
        }

        var ownerUserId = await DbContext.Instructors
            .AsNoTracking()
            .Where(instructor => instructor.Id == session.InstructorId)
            .Select(instructor => instructor.ApplicationUserId)
            .FirstOrDefaultAsync(cancellationToken);

        return string.Equals(ownerUserId, userId, StringComparison.Ordinal)
            ? null
            : OperationErrors.Dependency("ErrorLectureInstructorOwnership");
    }

    private SessionCode CreateSessionCode(DateTimeOffset nowUtc)
    {
        var plain = _sessionCodeService.GeneratePlainCode(_options.SessionCodeLength);
        return new SessionCode(
            plain,
            _sessionCodeService.HashCode(plain),
            _sessionCodeService.ProtectCode(plain),
            nowUtc.AddMinutes(_options.SessionCodeLifetimeMinutes));
    }

    private void AddEvent(
        Guid sessionId,
        LectureSessionEventType eventType,
        string performedByUserId,
        LectureSessionStatus? previousStatus,
        LectureSessionStatus? newStatus,
        string safeDescription)
        => DbContext.LectureSessionEvents.Add(new LectureSessionEvent(
            sessionId,
            eventType,
            _dateTimeProvider.UtcNow,
            performedByUserId,
            previousStatus,
            newStatus,
            safeDescription));

    private sealed record StartValidation(
        OperationError? Error,
        DateOnly SessionDate,
        DateTimeOffset ScheduledStartUtc,
        DateTimeOffset ScheduledEndUtc)
    {
        public static StartValidation Failure(OperationError error)
            => new(error, default, default, default);
    }

    private sealed record SessionCode(
        string Plain,
        string Hash,
        string Protected,
        DateTimeOffset ExpiresAtUtc);
}
