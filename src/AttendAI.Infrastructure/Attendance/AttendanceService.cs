using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using AttendAI.Application.Attendance;
using AttendAI.Application.Biometrics;
using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.Common.Models;
using AttendAI.Application.FaceVerification;
using AttendAI.Application.Lectures;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Attendance;

public sealed class AttendanceService : AcademicServiceBase, IAttendanceService
{
    private readonly IOneToOneFaceVerifier _oneToOneFaceVerifier;
    private readonly IFaceEngineDiagnosticsService _diagnosticsService;
    private readonly ILocationVerificationService _locationVerificationService;
    private readonly IAttendanceRateLimiter _rateLimiter;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IApplicationTimeZoneService _timeZoneService;
    private readonly AttendanceOptions _attendanceOptions;
    private readonly FaceVerificationOptions _faceVerificationOptions;

    public AttendanceService(
        ApplicationDbContext dbContext,
        IOneToOneFaceVerifier oneToOneFaceVerifier,
        IFaceEngineDiagnosticsService diagnosticsService,
        ILocationVerificationService locationVerificationService,
        IAttendanceRateLimiter rateLimiter,
        IDateTimeProvider dateTimeProvider,
        IApplicationTimeZoneService timeZoneService,
        IOptions<AttendanceOptions> attendanceOptions,
        IOptions<FaceVerificationOptions> faceVerificationOptions)
        : base(dbContext)
    {
        _oneToOneFaceVerifier = oneToOneFaceVerifier;
        _diagnosticsService = diagnosticsService;
        _locationVerificationService = locationVerificationService;
        _rateLimiter = rateLimiter;
        _dateTimeProvider = dateTimeProvider;
        _timeZoneService = timeZoneService;
        _attendanceOptions = attendanceOptions.Value;
        _faceVerificationOptions = faceVerificationOptions.Value;
    }

    public async Task<StudentAttendancePageDto> GetStudentPageAsync(string userId, CancellationToken cancellationToken = default)
    {
        var diagnostics = _diagnosticsService.GetDiagnostics();
        var student = await ResolveStudentAsync(userId, cancellationToken);
        if (student is null)
        {
            return new StudentAttendancePageDto(diagnostics, [], []);
        }

        var attempts = await GetStudentAttemptsAsync(student.StudentId, 10, cancellationToken);
        if (!student.AccountIsActive || !student.StudentIsActive || student.MustChangePassword)
        {
            return new StudentAttendancePageDto(diagnostics, [], attempts);
        }

        var sectionIds = await DbContext.StudentEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentId == student.StudentId && enrollment.IsActive)
            .Select(enrollment => enrollment.SectionId)
            .ToListAsync(cancellationToken);

        var nowUtc = _dateTimeProvider.UtcNow;
        var sessions = await DbContext.LectureSessions
            .AsNoTracking()
            .Include(session => session.Section)
                .ThenInclude(section => section!.Course)
            .Include(session => session.Instructor)
            .Include(session => session.Classroom)
            .Where(session =>
                sectionIds.Contains(session.SectionId) &&
                session.Status == LectureSessionStatus.Active &&
                session.AttendanceCheckInEnabled &&
                session.ActualStartUtc <= nowUtc &&
                session.ScheduledEndUtc >= nowUtc)
            .ToListAsync(cancellationToken);
        sessions = sessions.OrderBy(session => session.ScheduledStartUtc).ToList();

        var sessionIds = sessions.Select(session => session.Id).ToList();
        var checkedInSessionIds = await DbContext.AttendanceRecords
            .AsNoTracking()
            .Where(record => record.StudentId == student.StudentId && sessionIds.Contains(record.LectureSessionId))
            .Select(record => record.LectureSessionId)
            .ToListAsync(cancellationToken);

        var rows = sessions
            .Select(session => ToSessionDto(session, checkedInSessionIds.Contains(session.Id), diagnostics))
            .ToList();

        return new StudentAttendancePageDto(diagnostics, rows, attempts);
    }

    public async Task<OperationResult<AttendanceChallengeDto>> IssueChallengeAsync(
        string userId,
        Guid lectureSessionId,
        CancellationToken cancellationToken = default)
    {
        var student = await ResolveStudentAsync(userId, cancellationToken);
        if (student is null)
        {
            return OperationResult<AttendanceChallengeDto>.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var validation = await ValidateStudentAndSessionAsync(student, lectureSessionId, requireNoExistingRecord: true, cancellationToken);
        if (validation.Error is not null)
        {
            return OperationResult<AttendanceChallengeDto>.Failure(validation.Error);
        }

        var diagnostics = _diagnosticsService.GetDiagnostics();
        if (!RealFaceEngineReady(diagnostics))
        {
            return OperationResult<AttendanceChallengeDto>.Failure(OperationErrors.Dependency("ErrorAttendanceRealFaceEngineRequired"));
        }

        var nowUtc = _dateTimeProvider.UtcNow;
        var token = GenerateChallengeToken();
        var challenge = new AttendanceChallenge(
            student.StudentId,
            lectureSessionId,
            HashSecret(token),
            nowUtc,
            nowUtc.AddMinutes(Math.Max(1, _attendanceOptions.ChallengeLifetimeMinutes)));

        DbContext.AttendanceChallenges.Add(challenge);
        var saveResult = await SaveChangesAsync(cancellationToken);
        if (!saveResult.Succeeded)
        {
            return OperationResult<AttendanceChallengeDto>.Failure(saveResult.Errors.ToArray());
        }

        return OperationResult<AttendanceChallengeDto>.Success(ToChallengeDto(validation.Session!, token, challenge.ExpiresAtUtc, diagnostics));
    }

    public async Task<OperationResult<AttendanceCheckInResultDto>> CheckInAsync(
        string userId,
        AttendanceCheckInCommand command,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var nowUtc = _dateTimeProvider.UtcNow;
        var student = await ResolveStudentAsync(userId, cancellationToken);
        if (student is null)
        {
            return OperationResult<AttendanceCheckInResultDto>.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var idempotency = NormalizeIdempotencyKey(command.IdempotencyKey, Math.Max(1, _attendanceOptions.IdempotencyKeyMaximumLength));
        if (idempotency.IsValid)
        {
            var existingAttempt = await DbContext.AttendanceAttempts
                .AsNoTracking()
                .Where(attempt =>
                    attempt.StudentId == student.StudentId &&
                    attempt.LectureSessionId == command.LectureSessionId &&
                    attempt.IdempotencyKeyHash == idempotency.Hash)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingAttempt is not null)
            {
                return OperationResult<AttendanceCheckInResultDto>.Success(await ToResultDtoAsync(existingAttempt, cancellationToken));
            }
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var validation = await ValidateStudentAndSessionAsync(student, command.LectureSessionId, requireNoExistingRecord: false, cancellationToken);
        if (validation.Error is not null)
        {
            if (validation.Session is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return OperationResult<AttendanceCheckInResultDto>.Failure(validation.Error);
            }

            var rejected = await PersistAttemptAsync(
                student.StudentId,
                validation.Session.Id,
                idempotency.Hash,
                string.Empty,
                AttendanceAttemptOutcome.Rejected,
                MapValidationFailure(validation.Error.MessageKey),
                LocationVerificationOutcome.NotEvaluated,
                SafeDescriptionFor(validation.Error.MessageKey),
                stopwatch,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return rejected;
        }

        var session = validation.Session!;

        if (!idempotency.IsValid)
        {
            var invalid = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                string.Empty,
                AttendanceAttemptOutcome.Rejected,
                AttendanceFailureReason.IdempotencyKeyInvalid,
                LocationVerificationOutcome.NotEvaluated,
                "AttendanceAttemptDescriptionInvalidIdempotency",
                stopwatch,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return invalid;
        }

        if (!_rateLimiter.TryAcquire(userId, nowUtc))
        {
            var rateLimited = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                string.Empty,
                AttendanceAttemptOutcome.Rejected,
                AttendanceFailureReason.RateLimited,
                LocationVerificationOutcome.NotEvaluated,
                "AttendanceAttemptDescriptionRateLimited",
                stopwatch,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return rateLimited;
        }

        var existingRecord = await DbContext.AttendanceRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.StudentId == student.StudentId && record.LectureSessionId == session.Id, cancellationToken);
        if (existingRecord is not null)
        {
            var duplicate = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                string.Empty,
                AttendanceAttemptOutcome.Duplicate,
                AttendanceFailureReason.DuplicateAttendance,
                LocationVerificationOutcome.NotEvaluated,
                "AttendanceAttemptDescriptionDuplicate",
                stopwatch,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return duplicate;
        }

        var challengeHash = HashSecret(command.ChallengeToken ?? string.Empty);
        var challengeValidation = await ValidateChallengeAsync(student.StudentId, session.Id, command.ChallengeToken, challengeHash, nowUtc, cancellationToken);
        if (challengeValidation.Error is not null)
        {
            var rejected = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                challengeHash,
                AttendanceAttemptOutcome.Rejected,
                challengeValidation.FailureReason,
                LocationVerificationOutcome.NotEvaluated,
                challengeValidation.SafeDescription,
                stopwatch,
                null,
                null,
                null,
                null,
                null,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return rejected;
        }

        var policy = new AttendanceLocationPolicy(
            session.AttendanceLatitude,
            session.AttendanceLongitude,
            session.AllowedRadiusMeters,
            Math.Max(1, session.MaximumAcceptedAccuracyMeters),
            session.LocationVerificationRequired);
        var location = _locationVerificationService.Verify(command.Location, policy);
        if (!location.Accepted)
        {
            var rejected = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                challengeHash,
                AttendanceAttemptOutcome.Rejected,
                location.FailureReason,
                location.Outcome,
                SafeDescriptionFor(location.MessageKey),
                stopwatch,
                challengeValidation.Challenge,
                location.DistanceMeters,
                DecimalRound(command.Location.AccuracyMeters),
                session.AllowedRadiusMeters,
                session.MaximumAcceptedAccuracyMeters,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return rejected;
        }

        var diagnostics = _diagnosticsService.GetDiagnostics();
        if (!RealFaceEngineReady(diagnostics))
        {
            var rejected = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                challengeHash,
                AttendanceAttemptOutcome.Rejected,
                AttendanceFailureReason.FaceEngineUnavailable,
                location.Outcome,
                "AttendanceAttemptDescriptionRealEngineRequired",
                stopwatch,
                challengeValidation.Challenge,
                location.DistanceMeters,
                DecimalRound(command.Location.AccuracyMeters),
                session.AllowedRadiusMeters,
                session.MaximumAcceptedAccuracyMeters,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return rejected;
        }

        var verification = await _oneToOneFaceVerifier.VerifyAsync(
            student.StudentId,
            command.Capture,
            new FaceVerificationContext(FaceVerificationPurpose.FutureAttendance, userId, idempotency.Hash[..Math.Min(80, idempotency.Hash.Length)]),
            cancellationToken);

        if (verification.Outcome == FaceVerificationOutcome.IncompatibleTemplate && verification.FaceTemplateId.HasValue)
        {
            await MarkTemplateForReEnrollmentAsync(verification.FaceTemplateId.Value, userId, cancellationToken);
        }

        var faceAttempt = CreateFaceAttempt(student.StudentId, verification, idempotency.Hash);
        DbContext.FaceVerificationAttempts.Add(faceAttempt);

        if (verification.Decision != FaceVerificationDecision.Match || verification.Outcome != FaceVerificationOutcome.Matched)
        {
            var rejected = await PersistAttemptAsync(
                student.StudentId,
                session.Id,
                idempotency.Hash,
                challengeHash,
                AttendanceAttemptOutcome.Rejected,
                MapFaceFailure(verification.Outcome),
                location.Outcome,
                SafeDescriptionKey(verification.Outcome),
                stopwatch,
                challengeValidation.Challenge,
                location.DistanceMeters,
                DecimalRound(command.Location.AccuracyMeters),
                session.AllowedRadiusMeters,
                session.MaximumAcceptedAccuracyMeters,
                cancellationToken,
                faceAttempt);
            await transaction.CommitAsync(cancellationToken);
            return rejected;
        }

        var status = nowUtc <= session.ActualStartUtc.AddMinutes(session.LateThresholdMinutes)
            ? AttendanceStatus.Present
            : AttendanceStatus.Late;
        var attempt = new AttendanceAttempt(
            student.StudentId,
            session.Id,
            AttendanceAttemptOutcome.Succeeded,
            AttendanceFailureReason.None,
            location.Outcome,
            nowUtc,
            idempotency.Hash,
            challengeHash,
            "AttendanceAttemptDescriptionSucceeded",
            location.DistanceMeters,
            DecimalRound(command.Location.AccuracyMeters),
            session.AllowedRadiusMeters,
            session.MaximumAcceptedAccuracyMeters,
            Elapsed(stopwatch));
        attempt.AttachFaceVerificationAttempt(faceAttempt.Id);

        var record = new AttendanceRecord(
            session.Id,
            student.StudentId,
            attempt.Id,
            faceAttempt.Id,
            status,
            nowUtc,
            session.ClassroomId,
            session.AllowedRadiusMeters,
            session.MaximumAcceptedAccuracyMeters,
            location.DistanceMeters ?? 0m,
            DecimalRound(command.Location.AccuracyMeters));
        challengeValidation.Challenge!.Consume(nowUtc, attempt.Id);

        DbContext.AttendanceAttempts.Add(attempt);
        DbContext.AttendanceRecords.Add(record);

        var saveResult = await SaveChangesAsync(cancellationToken);
        if (!saveResult.Succeeded)
        {
            return OperationResult<AttendanceCheckInResultDto>.Failure(saveResult.Errors.ToArray());
        }

        await transaction.CommitAsync(cancellationToken);
        return OperationResult<AttendanceCheckInResultDto>.Success(await ToResultDtoAsync(attempt, cancellationToken));
    }

    public async Task<InstructorAttendanceRosterDto?> GetInstructorRosterAsync(
        string userId,
        Guid lectureSessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await DbContext.LectureSessions
            .AsNoTracking()
            .Include(item => item.Section)
                .ThenInclude(section => section!.Course)
            .Include(item => item.Classroom)
            .Include(item => item.Instructor)
            .FirstOrDefaultAsync(item => item.Id == lectureSessionId, cancellationToken);

        if (session is null || session.Instructor is null || !string.Equals(session.Instructor.ApplicationUserId, userId, StringComparison.Ordinal))
        {
            return null;
        }

        var studentRows = await (
            from enrollment in DbContext.StudentEnrollments.AsNoTracking()
            join student in DbContext.Students.AsNoTracking() on enrollment.StudentId equals student.Id
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where enrollment.SectionId == session.SectionId && enrollment.IsActive && student.IsActive
            orderby student.StudentNumber
            select new StudentRosterRow(
                student.Id,
                student.StudentNumber,
                student.NameEnglish,
                student.NameArabic,
                user.Email ?? string.Empty))
            .ToListAsync(cancellationToken);

        var studentIds = studentRows.Select(row => row.StudentId).ToList();
        var records = await DbContext.AttendanceRecords
            .AsNoTracking()
            .Where(record => record.LectureSessionId == lectureSessionId && studentIds.Contains(record.StudentId))
            .ToListAsync(cancellationToken);
        var attempts = await DbContext.AttendanceAttempts
            .AsNoTracking()
            .Where(attempt => attempt.LectureSessionId == lectureSessionId && studentIds.Contains(attempt.StudentId))
            .ToListAsync(cancellationToken);
        attempts = attempts.OrderByDescending(attempt => attempt.AttemptedAtUtc).ToList();
        var faceAttemptIds = records
            .Select(record => record.FaceVerificationAttemptId)
            .Concat(attempts.Where(attempt => attempt.FaceVerificationAttemptId.HasValue).Select(attempt => attempt.FaceVerificationAttemptId!.Value))
            .Distinct()
            .ToList();
        var faceAttempts = await DbContext.FaceVerificationAttempts
            .AsNoTracking()
            .Where(attempt => faceAttemptIds.Contains(attempt.Id))
            .ToDictionaryAsync(attempt => attempt.Id, cancellationToken);

        var rows = studentRows.Select(student =>
        {
            var record = records.FirstOrDefault(item => item.StudentId == student.StudentId);
            var lastAttempt = attempts.FirstOrDefault(item => item.StudentId == student.StudentId);
            FaceVerificationAttempt? faceAttempt = null;
            if (record is not null)
            {
                faceAttempts.TryGetValue(record.FaceVerificationAttemptId, out faceAttempt);
            }
            else if (lastAttempt?.FaceVerificationAttemptId is Guid faceAttemptId)
            {
                faceAttempts.TryGetValue(faceAttemptId, out faceAttempt);
            }

            return new InstructorAttendanceRosterRowDto(
                student.StudentId,
                student.StudentNumber,
                student.NameEnglish,
                student.NameArabic,
                student.Email,
                record?.Status,
                record?.CheckedInAtUtc,
                record is null ? null : _timeZoneService.ConvertUtcToLocal(record.CheckedInAtUtc),
                record?.DistanceMeters,
                record?.BrowserAccuracyMeters,
                faceAttempt?.Decision,
                lastAttempt?.Outcome,
                lastAttempt?.FailureReason,
                lastAttempt is null ? null : _timeZoneService.ConvertUtcToLocal(lastAttempt.AttemptedAtUtc));
        }).ToList();

        var present = rows.Count(row => row.Status == AttendanceStatus.Present);
        var late = rows.Count(row => row.Status == AttendanceStatus.Late);
        return new InstructorAttendanceRosterDto(
            session.Id,
            session.Section?.Course?.Code ?? string.Empty,
            session.Section?.Course?.NameEnglish ?? string.Empty,
            session.Section?.Course?.NameArabic ?? string.Empty,
            session.Section?.SectionNumber ?? string.Empty,
            session.Classroom?.Code ?? string.Empty,
            _timeZoneService.ConvertUtcToLocal(session.ScheduledStartUtc),
            _timeZoneService.ConvertUtcToLocal(session.ScheduledEndUtc),
            rows.Count,
            present,
            late,
            rows.Count - present - late,
            rows);
    }

    private async Task<IReadOnlyList<StudentAttendanceAttemptDto>> GetStudentAttemptsAsync(
        Guid studentId,
        int count,
        CancellationToken cancellationToken)
    {
        var attempts = await DbContext.AttendanceAttempts
            .AsNoTracking()
            .Include(attempt => attempt.LectureSession)
                .ThenInclude(session => session!.Section)
                    .ThenInclude(section => section!.Course)
            .Include(attempt => attempt.LectureSession)
                .ThenInclude(session => session!.Classroom)
            .Where(attempt => attempt.StudentId == studentId)
            .ToListAsync(cancellationToken);
        attempts = attempts.OrderByDescending(attempt => attempt.AttemptedAtUtc).Take(count).ToList();

        var attemptIds = attempts.Select(attempt => attempt.Id).ToList();
        var records = await DbContext.AttendanceRecords
            .AsNoTracking()
            .Where(record => attemptIds.Contains(record.AttendanceAttemptId))
            .ToDictionaryAsync(record => record.AttendanceAttemptId, cancellationToken);

        return attempts.Select(attempt =>
        {
            records.TryGetValue(attempt.Id, out var record);
            return new StudentAttendanceAttemptDto(
                attempt.Id,
                attempt.LectureSessionId,
                attempt.LectureSession?.Section?.Course?.Code ?? string.Empty,
                attempt.LectureSession?.Section?.SectionNumber ?? string.Empty,
                attempt.LectureSession?.Classroom?.Code ?? string.Empty,
                attempt.Outcome,
                attempt.FailureReason,
                attempt.LocationOutcome,
                attempt.AttemptedAtUtc,
                _timeZoneService.ConvertUtcToLocal(attempt.AttemptedAtUtc),
                attempt.DistanceMeters,
                attempt.BrowserAccuracyMeters,
                record?.Id,
                attempt.FaceVerificationAttemptId);
        }).ToList();
    }

    private async Task<ValidationResult> ValidateStudentAndSessionAsync(
        StudentUserRow student,
        Guid lectureSessionId,
        bool requireNoExistingRecord,
        CancellationToken cancellationToken)
    {
        if (!student.AccountIsActive || !student.StudentIsActive)
        {
            return ValidationResult.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        if (student.MustChangePassword)
        {
            return ValidationResult.Failure(OperationErrors.Dependency("ErrorAttendancePasswordChangeRequired"));
        }

        var session = await DbContext.LectureSessions
            .Include(item => item.Section)
                .ThenInclude(section => section!.Course)
            .Include(item => item.Instructor)
            .Include(item => item.Classroom)
            .FirstOrDefaultAsync(item => item.Id == lectureSessionId, cancellationToken);
        if (session is null)
        {
            return ValidationResult.Failure(OperationErrors.NotFound("ErrorLectureSessionNotFound"));
        }

        var enrolled = await DbContext.StudentEnrollments.AnyAsync(enrollment =>
            enrollment.StudentId == student.StudentId &&
            enrollment.SectionId == session.SectionId &&
            enrollment.IsActive,
            cancellationToken);
        if (!enrolled)
        {
            return ValidationResult.Failure(OperationErrors.Dependency("ErrorAttendanceEnrollmentRequired"), session);
        }

        var nowUtc = _dateTimeProvider.UtcNow;
        if (session.Status != LectureSessionStatus.Active || !session.AttendanceCheckInEnabled)
        {
            return ValidationResult.Failure(OperationErrors.Dependency("ErrorAttendanceSessionNotActive"), session);
        }

        if (session.ActualStartUtc > nowUtc || session.ScheduledEndUtc < nowUtc)
        {
            return ValidationResult.Failure(OperationErrors.Dependency("ErrorAttendanceWindowClosed"), session);
        }

        if (session.LocationVerificationRequired && (!session.AttendanceLatitude.HasValue || !session.AttendanceLongitude.HasValue))
        {
            return ValidationResult.Failure(OperationErrors.Dependency("ErrorAttendanceLocationPolicyMissing"), session);
        }

        if (requireNoExistingRecord)
        {
            var existing = await DbContext.AttendanceRecords.AnyAsync(record =>
                record.StudentId == student.StudentId &&
                record.LectureSessionId == session.Id,
                cancellationToken);
            if (existing)
            {
                return ValidationResult.Failure(OperationErrors.Duplicate(nameof(AttendanceRecord), "ErrorAttendanceAlreadyRecorded"), session);
            }
        }

        return new ValidationResult(null, session);
    }

    private async Task<ChallengeValidation> ValidateChallengeAsync(
        Guid studentId,
        Guid lectureSessionId,
        string? challengeToken,
        string challengeHash,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return ChallengeValidation.Failure(AttendanceFailureReason.ChallengeMissing, "AttendanceAttemptDescriptionChallengeMissing");
        }

        var challenge = await DbContext.AttendanceChallenges
            .FirstOrDefaultAsync(item =>
                item.StudentId == studentId &&
                item.LectureSessionId == lectureSessionId &&
                item.TokenHash == challengeHash,
                cancellationToken);

        if (challenge is null)
        {
            return ChallengeValidation.Failure(AttendanceFailureReason.ChallengeInvalid, "AttendanceAttemptDescriptionChallengeInvalid");
        }

        if (challenge.IsConsumed)
        {
            return ChallengeValidation.Failure(AttendanceFailureReason.ChallengeAlreadyUsed, "AttendanceAttemptDescriptionChallengeAlreadyUsed");
        }

        if (challenge.IsExpired(nowUtc))
        {
            return ChallengeValidation.Failure(AttendanceFailureReason.ChallengeExpired, "AttendanceAttemptDescriptionChallengeExpired");
        }

        return new ChallengeValidation(null, AttendanceFailureReason.None, string.Empty, challenge);
    }

    private async Task<OperationResult<AttendanceCheckInResultDto>> PersistAttemptAsync(
        Guid studentId,
        Guid lectureSessionId,
        string idempotencyKeyHash,
        string challengeTokenHash,
        AttendanceAttemptOutcome outcome,
        AttendanceFailureReason failureReason,
        LocationVerificationOutcome locationOutcome,
        string safeDescription,
        Stopwatch stopwatch,
        AttendanceChallenge? challenge,
        decimal? distanceMeters,
        decimal? browserAccuracyMeters,
        int? allowedRadiusMeters,
        int? maximumAcceptedAccuracyMeters,
        CancellationToken cancellationToken,
        FaceVerificationAttempt? faceAttempt = null)
    {
        var attempt = new AttendanceAttempt(
            studentId,
            lectureSessionId,
            outcome,
            failureReason,
            locationOutcome,
            _dateTimeProvider.UtcNow,
            idempotencyKeyHash,
            challengeTokenHash,
            safeDescription,
            distanceMeters,
            browserAccuracyMeters,
            allowedRadiusMeters,
            maximumAcceptedAccuracyMeters,
            Elapsed(stopwatch));

        if (faceAttempt is not null)
        {
            attempt.AttachFaceVerificationAttempt(faceAttempt.Id);
        }

        if (challenge is not null && (_attendanceOptions.ConsumeChallengeOnRejectedAttempt || outcome == AttendanceAttemptOutcome.Succeeded))
        {
            challenge.Consume(_dateTimeProvider.UtcNow, attempt.Id);
        }

        DbContext.AttendanceAttempts.Add(attempt);
        var saveResult = await SaveChangesAsync(cancellationToken);
        if (!saveResult.Succeeded)
        {
            return OperationResult<AttendanceCheckInResultDto>.Failure(saveResult.Errors.ToArray());
        }

        return OperationResult<AttendanceCheckInResultDto>.Success(await ToResultDtoAsync(attempt, cancellationToken));
    }

    private async Task<AttendanceCheckInResultDto> ToResultDtoAsync(AttendanceAttempt attempt, CancellationToken cancellationToken)
    {
        var record = await DbContext.AttendanceRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.AttendanceAttemptId == attempt.Id, cancellationToken)
            ?? await DbContext.AttendanceRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.StudentId == attempt.StudentId && item.LectureSessionId == attempt.LectureSessionId, cancellationToken);
        FaceVerificationAttempt? faceAttempt = null;
        if (attempt.FaceVerificationAttemptId.HasValue)
        {
            faceAttempt = await DbContext.FaceVerificationAttempts
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == attempt.FaceVerificationAttemptId.Value, cancellationToken);
        }

        return new AttendanceCheckInResultDto(
            attempt.Id,
            record?.Id,
            attempt.FaceVerificationAttemptId,
            attempt.Outcome == AttendanceAttemptOutcome.Succeeded,
            attempt.Outcome == AttendanceAttemptOutcome.Duplicate,
            record?.Status,
            attempt.FailureReason,
            attempt.LocationOutcome,
            MessageKey(attempt),
            attempt.AttemptedAtUtc,
            record?.CheckedInAtUtc,
            attempt.DistanceMeters ?? record?.DistanceMeters,
            attempt.BrowserAccuracyMeters ?? record?.BrowserAccuracyMeters,
            faceAttempt?.Score,
            faceAttempt?.Threshold,
            faceAttempt?.Decision ?? FaceVerificationDecision.Unknown,
            _diagnosticsService.GetDiagnostics().IsDemoMode);
    }

    private AttendanceSessionDto ToSessionDto(LectureSession session, bool alreadyCheckedIn, FaceEngineDiagnosticsDto diagnostics)
    {
        var locationPolicyAvailable = !session.LocationVerificationRequired ||
            (session.AttendanceLatitude.HasValue && session.AttendanceLongitude.HasValue);
        var engineReady = RealFaceEngineReady(diagnostics);
        var canCheckIn = _attendanceOptions.AttendanceEnabled &&
            session.AttendanceCheckInEnabled &&
            locationPolicyAvailable &&
            engineReady &&
            !alreadyCheckedIn;

        var messageKey = alreadyCheckedIn
            ? "ErrorAttendanceAlreadyRecorded"
            : !locationPolicyAvailable
                ? "ErrorAttendanceLocationPolicyMissing"
                : !engineReady
                    ? "ErrorAttendanceRealFaceEngineRequired"
                    : "AttendanceCheckInReady";

        return new AttendanceSessionDto(
            session.Id,
            session.Section?.Course?.Code ?? string.Empty,
            session.Section?.Course?.NameEnglish ?? string.Empty,
            session.Section?.Course?.NameArabic ?? string.Empty,
            session.Section?.SectionNumber ?? string.Empty,
            session.Instructor?.NameEnglish ?? string.Empty,
            session.Instructor?.NameArabic ?? string.Empty,
            session.Classroom?.Code ?? string.Empty,
            session.ScheduledStartUtc,
            session.ScheduledEndUtc,
            _timeZoneService.ConvertUtcToLocal(session.ScheduledStartUtc),
            _timeZoneService.ConvertUtcToLocal(session.ScheduledEndUtc),
            session.ActualStartUtc,
            _timeZoneService.ConvertUtcToLocal(session.ActualStartUtc),
            session.LateThresholdMinutes,
            session.AllowedRadiusMeters,
            session.MaximumAcceptedAccuracyMeters,
            locationPolicyAvailable,
            alreadyCheckedIn,
            canCheckIn,
            messageKey);
    }

    private AttendanceChallengeDto ToChallengeDto(LectureSession session, string challengeToken, DateTimeOffset expiresAtUtc, FaceEngineDiagnosticsDto diagnostics)
        => new(
            ToSessionDto(session, alreadyCheckedIn: false, diagnostics),
            challengeToken,
            expiresAtUtc,
            _timeZoneService.ConvertUtcToLocal(expiresAtUtc),
            _faceVerificationOptions.MaximumCaptureBytes,
            string.Join(",", _faceVerificationOptions.AllowedMimeTypes),
            Math.Max(1, session.MaximumAcceptedAccuracyMeters));

    private FaceVerificationAttempt CreateFaceAttempt(
        Guid studentId,
        OneToOneVerificationResult verification,
        string idempotencyKeyHash)
        => new(
            studentId,
            verification.FaceTemplateId,
            FaceVerificationPurpose.FutureAttendance,
            verification.Outcome,
            verification.Decision,
            verification.ErrorCode,
            verification.Score,
            verification.Threshold,
            verification.ScoreMetric,
            verification.EngineName,
            verification.EngineVersion,
            verification.ModelName,
            verification.ModelVersion,
            verification.TemplateFormatVersion,
            _dateTimeProvider.UtcNow,
            idempotencyKeyHash[..Math.Min(80, idempotencyKeyHash.Length)],
            SafeDescriptionKey(verification.Outcome),
            verification.ImageWidth,
            verification.ImageHeight,
            verification.DetectedFaceCount,
            verification.QualityScore,
            verification.ProcessingDurationMilliseconds);

    private async Task MarkTemplateForReEnrollmentAsync(
        Guid faceTemplateId,
        string userId,
        CancellationToken cancellationToken)
    {
        var template = await DbContext.StudentFaceTemplates
            .Where(item => item.Id == faceTemplateId && item.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        template?.Revoke(
            _dateTimeProvider.UtcNow,
            userId,
            "Template is incompatible with the configured attendance face verification engine.",
            requiresReEnrollment: true);
    }

    private async Task<StudentUserRow?> ResolveStudentAsync(string userId, CancellationToken cancellationToken)
        => await (
            from student in DbContext.Students.AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.ApplicationUserId == userId
            select new StudentUserRow(
                student.Id,
                student.IsActive,
                user.IsActive && !user.IsDisabled,
                user.MustChangePassword))
            .FirstOrDefaultAsync(cancellationToken);

    private static bool RealFaceEngineReady(FaceEngineDiagnosticsDto diagnostics)
        => diagnostics.Mode.Equals("Real", StringComparison.OrdinalIgnoreCase) &&
           diagnostics.VerificationEnabled &&
           diagnostics.IsProductionSafe &&
           !diagnostics.IsDemoMode;

    private static NormalizedIdempotencyKey NormalizeIdempotencyKey(string? idempotencyKey, int maximumLength)
    {
        var normalized = idempotencyKey?.Trim() ?? string.Empty;
        if (normalized.Length > 0 && normalized.Length <= maximumLength)
        {
            return new NormalizedIdempotencyKey(true, HashSecret(normalized));
        }

        return new NormalizedIdempotencyKey(false, HashSecret($"invalid-{Guid.NewGuid():N}"));
    }

    private static string GenerateChallengeToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string HashSecret(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static int Elapsed(Stopwatch stopwatch)
        => (int)Math.Min(int.MaxValue, stopwatch.ElapsedMilliseconds);

    private static decimal DecimalRound(double value)
        => Math.Round((decimal)value, 3, MidpointRounding.AwayFromZero);

    private static AttendanceFailureReason MapValidationFailure(string messageKey)
        => messageKey switch
        {
            "ErrorAttendancePasswordChangeRequired" => AttendanceFailureReason.PasswordChangeRequired,
            "ErrorLectureSessionNotFound" => AttendanceFailureReason.SessionNotFound,
            "ErrorAttendanceEnrollmentRequired" => AttendanceFailureReason.NotEnrolled,
            "ErrorAttendanceSessionNotActive" => AttendanceFailureReason.SessionNotActive,
            "ErrorAttendanceWindowClosed" => AttendanceFailureReason.AttendanceWindowClosed,
            "ErrorAttendanceLocationPolicyMissing" => AttendanceFailureReason.AttendanceLocationUnavailable,
            "ErrorAttendanceAlreadyRecorded" => AttendanceFailureReason.DuplicateAttendance,
            _ => AttendanceFailureReason.StudentInactive
        };

    private static AttendanceFailureReason MapFaceFailure(FaceVerificationOutcome outcome)
        => outcome switch
        {
            FaceVerificationOutcome.NotMatched => AttendanceFailureReason.FaceNotMatched,
            FaceVerificationOutcome.NoFace => AttendanceFailureReason.FaceNoFace,
            FaceVerificationOutcome.MultipleFaces => AttendanceFailureReason.FaceMultipleFaces,
            FaceVerificationOutcome.LowQuality => AttendanceFailureReason.FaceLowQuality,
            FaceVerificationOutcome.RateLimited => AttendanceFailureReason.RateLimited,
            FaceVerificationOutcome.EngineUnavailable => AttendanceFailureReason.FaceEngineUnavailable,
            _ => AttendanceFailureReason.FaceVerificationFailed
        };

    private static string SafeDescriptionKey(FaceVerificationOutcome outcome)
        => outcome switch
        {
            FaceVerificationOutcome.Matched => "AttendanceFaceVerificationDescriptionMatched",
            FaceVerificationOutcome.NotMatched => "AttendanceFaceVerificationDescriptionNotMatched",
            FaceVerificationOutcome.NoActiveConsent => "AttendanceFaceVerificationDescriptionNoActiveConsent",
            FaceVerificationOutcome.NoActiveTemplate => "AttendanceFaceVerificationDescriptionNoActiveTemplate",
            FaceVerificationOutcome.RequiresReEnrollment => "AttendanceFaceVerificationDescriptionRequiresReEnrollment",
            FaceVerificationOutcome.IncompatibleTemplate => "AttendanceFaceVerificationDescriptionIncompatibleTemplate",
            FaceVerificationOutcome.NoFace => "AttendanceFaceVerificationDescriptionNoFace",
            FaceVerificationOutcome.MultipleFaces => "AttendanceFaceVerificationDescriptionMultipleFaces",
            FaceVerificationOutcome.LowQuality => "AttendanceFaceVerificationDescriptionLowQuality",
            FaceVerificationOutcome.InvalidImage => "AttendanceFaceVerificationDescriptionInvalidImage",
            FaceVerificationOutcome.RateLimited => "AttendanceFaceVerificationDescriptionRateLimited",
            FaceVerificationOutcome.EngineUnavailable => "AttendanceFaceVerificationDescriptionEngineUnavailable",
            FaceVerificationOutcome.ProcessingFailed => "AttendanceFaceVerificationDescriptionProcessingFailed",
            _ => "AttendanceFaceVerificationDescriptionCancelled"
        };

    private static string SafeDescriptionFor(string messageKey)
        => messageKey switch
        {
            "ErrorAttendanceLocationInvalid" => "AttendanceAttemptDescriptionLocationInvalid",
            "ErrorAttendanceLocationInaccurate" => "AttendanceAttemptDescriptionLocationInaccurate",
            "ErrorAttendanceOutsideGeofence" => "AttendanceAttemptDescriptionOutsideGeofence",
            "ErrorAttendanceLocationPolicyMissing" => "AttendanceAttemptDescriptionLocationPolicyMissing",
            "ErrorAttendanceWindowClosed" => "AttendanceAttemptDescriptionWindowClosed",
            "ErrorAttendanceSessionNotActive" => "AttendanceAttemptDescriptionSessionInactive",
            "ErrorAttendanceEnrollmentRequired" => "AttendanceAttemptDescriptionNotEnrolled",
            "ErrorAttendanceAlreadyRecorded" => "AttendanceAttemptDescriptionDuplicate",
            "ErrorAttendancePasswordChangeRequired" => "AttendanceAttemptDescriptionPasswordChangeRequired",
            _ => "AttendanceAttemptDescriptionRejected"
        };

    private static string MessageKey(AttendanceAttempt attempt)
        => attempt.Outcome switch
        {
            AttendanceAttemptOutcome.Succeeded => "AttendanceCheckInSucceeded",
            AttendanceAttemptOutcome.Duplicate => "ErrorAttendanceAlreadyRecorded",
            _ => attempt.FailureReason switch
            {
                AttendanceFailureReason.IdempotencyKeyInvalid => "ErrorAttendanceIdempotencyInvalid",
                AttendanceFailureReason.RateLimited => "ErrorAttendanceRateLimited",
                AttendanceFailureReason.PasswordChangeRequired => "ErrorAttendancePasswordChangeRequired",
                AttendanceFailureReason.NotEnrolled => "ErrorAttendanceEnrollmentRequired",
                AttendanceFailureReason.SessionNotFound => "ErrorLectureSessionNotFound",
                AttendanceFailureReason.SessionNotActive => "ErrorAttendanceSessionNotActive",
                AttendanceFailureReason.AttendanceWindowClosed => "ErrorAttendanceWindowClosed",
                AttendanceFailureReason.AttendanceLocationUnavailable => "ErrorAttendanceLocationPolicyMissing",
                AttendanceFailureReason.LocationInvalid => "ErrorAttendanceLocationInvalid",
                AttendanceFailureReason.LocationInaccurate => "ErrorAttendanceLocationInaccurate",
                AttendanceFailureReason.OutsideGeofence => "ErrorAttendanceOutsideGeofence",
                AttendanceFailureReason.ChallengeMissing => "ErrorAttendanceChallengeMissing",
                AttendanceFailureReason.ChallengeInvalid => "ErrorAttendanceChallengeInvalid",
                AttendanceFailureReason.ChallengeExpired => "ErrorAttendanceChallengeExpired",
                AttendanceFailureReason.ChallengeAlreadyUsed => "ErrorAttendanceChallengeAlreadyUsed",
                AttendanceFailureReason.FaceEngineUnavailable => "ErrorAttendanceRealFaceEngineRequired",
                AttendanceFailureReason.FaceNotMatched => "ErrorAttendanceFaceNotMatched",
                AttendanceFailureReason.FaceNoFace => "ErrorFaceVerificationNoFace",
                AttendanceFailureReason.FaceMultipleFaces => "ErrorFaceVerificationMultipleFaces",
                AttendanceFailureReason.FaceLowQuality => "ErrorFaceVerificationQualityTooLow",
                _ => "ErrorAttendanceCheckInRejected"
            }
        };

    private sealed record StudentUserRow(
        Guid StudentId,
        bool StudentIsActive,
        bool AccountIsActive,
        bool MustChangePassword);

    private sealed record ValidationResult(OperationError? Error, LectureSession? Session)
    {
        public static ValidationResult Failure(OperationError error)
            => new(error, null);

        public static ValidationResult Failure(OperationError error, LectureSession session)
            => new(error, session);
    }

    private sealed record ChallengeValidation(
        OperationError? Error,
        AttendanceFailureReason FailureReason,
        string SafeDescription,
        AttendanceChallenge? Challenge)
    {
        public static ChallengeValidation Failure(AttendanceFailureReason failureReason, string safeDescription)
            => new(OperationErrors.Dependency(MessageKey(failureReason)), failureReason, safeDescription, null);

        private static string MessageKey(AttendanceFailureReason failureReason)
            => failureReason switch
            {
                AttendanceFailureReason.ChallengeMissing => "ErrorAttendanceChallengeMissing",
                AttendanceFailureReason.ChallengeExpired => "ErrorAttendanceChallengeExpired",
                AttendanceFailureReason.ChallengeAlreadyUsed => "ErrorAttendanceChallengeAlreadyUsed",
                _ => "ErrorAttendanceChallengeInvalid"
            };
    }

    private sealed record NormalizedIdempotencyKey(bool IsValid, string Hash);

    private sealed record StudentRosterRow(
        Guid StudentId,
        string StudentNumber,
        string NameEnglish,
        string NameArabic,
        string Email);
}
