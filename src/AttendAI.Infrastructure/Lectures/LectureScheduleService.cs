using System.Data;
using System.Linq.Expressions;
using AttendAI.Application.Common.Models;
using AttendAI.Application.Lectures;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Lectures;

public sealed class LectureScheduleService : AcademicServiceBase, ILectureScheduleService
{
    private readonly LectureSchedulingOptions _options;

    public LectureScheduleService(ApplicationDbContext dbContext, IOptions<LectureSchedulingOptions> options)
        : base(dbContext)
    {
        _options = options.Value;
    }

    public async Task<PagedResult<LectureScheduleDto>> GetPagedAsync(LectureScheduleQuery query, CancellationToken cancellationToken = default)
    {
        var schedules = BaseScheduleQuery().AsNoTracking();

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            schedules = schedules.Where(schedule =>
                schedule.Section!.SectionNumber.Contains(search) ||
                schedule.Section.Course!.Code.Contains(search) ||
                schedule.Section.Course.NameEnglish.Contains(search) ||
                schedule.Section.Course.NameArabic.Contains(search) ||
                schedule.Instructor!.EmployeeNumber.Contains(search) ||
                schedule.Instructor.NameEnglish.Contains(search) ||
                schedule.Instructor.NameArabic.Contains(search) ||
                schedule.Classroom!.Code.Contains(search));
        }

        if (query.SectionId.HasValue)
        {
            schedules = schedules.Where(schedule => schedule.SectionId == query.SectionId.Value);
        }

        if (query.InstructorId.HasValue)
        {
            schedules = schedules.Where(schedule => schedule.InstructorId == query.InstructorId.Value);
        }

        if (query.ClassroomId.HasValue)
        {
            schedules = schedules.Where(schedule => schedule.ClassroomId == query.ClassroomId.Value);
        }

        if (query.IsActive.HasValue)
        {
            schedules = schedules.Where(schedule => schedule.IsActive == query.IsActive.Value);
        }

        if (query.DayOfWeek.HasValue)
        {
            schedules = schedules.Where(schedule => schedule.DayOfWeek == query.DayOfWeek.Value);
        }

        schedules = query.SortBy?.ToLowerInvariant() switch
        {
            "course" => query.SortDescending ? schedules.OrderByDescending(s => s.Section!.Course!.Code) : schedules.OrderBy(s => s.Section!.Course!.Code),
            "instructor" => query.SortDescending ? schedules.OrderByDescending(s => s.Instructor!.EmployeeNumber) : schedules.OrderBy(s => s.Instructor!.EmployeeNumber),
            "classroom" => query.SortDescending ? schedules.OrderByDescending(s => s.Classroom!.Code) : schedules.OrderBy(s => s.Classroom!.Code),
            "day" => query.SortDescending ? schedules.OrderByDescending(s => s.DayOfWeek) : schedules.OrderBy(s => s.DayOfWeek),
            "status" => query.SortDescending ? schedules.OrderByDescending(s => s.IsActive) : schedules.OrderBy(s => s.IsActive),
            _ => query.SortDescending ? schedules.OrderByDescending(s => s.StartTime) : schedules.OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
        };

        var total = await schedules.CountAsync(cancellationToken);
        var rows = await ApplyPaging(schedules, query).ToListAsync(cancellationToken);
        return new PagedResult<LectureScheduleDto>(
            rows.Select(LectureProjection.ToDto).ToList(),
            query.PageNumber,
            query.PageSize,
            total);
    }

    public async Task<LectureScheduleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schedule = await BaseScheduleQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return schedule is null ? null : LectureProjection.ToDto(schedule);
    }

    public async Task<OperationResult<Guid>> CreateAsync(LectureScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var validationError = await ValidateScheduleAsync(command, cancellationToken);
        if (validationError is not null)
        {
            return OperationResult<Guid>.Failure(validationError);
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var conflict = await FindConflictAsync(null, command, cancellationToken);
        if (conflict.HasConflict)
        {
            return OperationResult<Guid>.Failure(ConflictError(conflict));
        }

        LectureSchedule schedule;
        try
        {
            schedule = new LectureSchedule(
                command.SectionId,
                command.InstructorId,
                command.ClassroomId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime,
                command.EffectiveFrom,
                command.EffectiveTo,
                command.DefaultLateThresholdMinutes,
                command.DefaultAllowedRadiusMeters);
        }
        catch (ArgumentException exception)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.LectureSchedules.Add(schedule);
        var result = await SaveCreatedAsync(schedule.Id, cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public async Task<OperationResult> UpdateAsync(Guid id, LectureScheduleCommand command, CancellationToken cancellationToken = default)
    {
        var schedule = await DbContext.LectureSchedules.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (schedule is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var validationError = await ValidateScheduleAsync(command, cancellationToken);
        if (validationError is not null)
        {
            return OperationResult.Failure(validationError);
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var conflict = await FindConflictAsync(id, command, cancellationToken);
        if (conflict.HasConflict)
        {
            return OperationResult.Failure(ConflictError(conflict));
        }

        try
        {
            SetOriginalRowVersion(schedule, command.RowVersion);
            schedule.Update(
                command.SectionId,
                command.InstructorId,
                command.ClassroomId,
                command.DayOfWeek,
                command.StartTime,
                command.EndTime,
                command.EffectiveFrom,
                command.EffectiveTo,
                command.DefaultLateThresholdMinutes,
                command.DefaultAllowedRadiusMeters);
        }
        catch (ArgumentException exception)
        {
            return OperationResult.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        var result = await SaveChangesAsync(cancellationToken);
        if (!result.Succeeded)
        {
            return result;
        }

        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    public Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(id, true, cancellationToken);

    public Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(id, false, cancellationToken);

    public Task<ScheduleConflictResult> CheckConflictAsync(Guid? excludedScheduleId, LectureScheduleCommand command, CancellationToken cancellationToken = default)
        => FindConflictAsync(excludedScheduleId, command, cancellationToken);

    public async Task<IReadOnlyList<LectureScheduleDto>> GetClassroomTimetableAsync(Guid classroomId, CancellationToken cancellationToken = default)
    {
        var rows = await BaseScheduleQuery()
            .AsNoTracking()
            .Where(schedule => schedule.ClassroomId == classroomId && schedule.IsActive)
            .OrderBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);

        return rows.Select(LectureProjection.ToDto).ToList();
    }

    public async Task<IReadOnlyList<LectureScheduleDto>> GetSectionTimetableAsync(Guid sectionId, CancellationToken cancellationToken = default)
    {
        var rows = await BaseScheduleQuery()
            .AsNoTracking()
            .Where(schedule => schedule.SectionId == sectionId && schedule.IsActive)
            .OrderBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);

        return rows.Select(LectureProjection.ToDto).ToList();
    }

    private IQueryable<LectureSchedule> BaseScheduleQuery()
        => DbContext.LectureSchedules
            .Include(schedule => schedule.Section)
                .ThenInclude(section => section!.Course)
            .Include(schedule => schedule.Instructor)
            .Include(schedule => schedule.Classroom);

    private async Task<OperationResult> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var schedule = await DbContext.LectureSchedules.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (schedule is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (isActive)
        {
            schedule.Activate();
        }
        else
        {
            schedule.Deactivate();
        }

        return await SaveChangesAsync(cancellationToken);
    }

    private async Task<OperationError?> ValidateScheduleAsync(LectureScheduleCommand command, CancellationToken cancellationToken)
    {
        var durationMinutes = (int)(command.EndTime - command.StartTime).TotalMinutes;
        if (command.StartTime >= command.EndTime)
        {
            return OperationErrors.Validation(nameof(command.StartTime), "ErrorLectureStartBeforeEnd");
        }

        if (command.EffectiveFrom > command.EffectiveTo)
        {
            return OperationErrors.Validation(nameof(command.EffectiveFrom), "ErrorLectureEffectiveRange");
        }

        if (durationMinutes < _options.MinimumLectureDurationMinutes || durationMinutes > _options.MaximumLectureDurationMinutes)
        {
            return OperationErrors.Validation(
                nameof(command.EndTime),
                "ErrorLectureDurationRange",
                _options.MinimumLectureDurationMinutes,
                _options.MaximumLectureDurationMinutes);
        }

        if (command.DefaultLateThresholdMinutes < 0 || command.DefaultLateThresholdMinutes >= durationMinutes)
        {
            return OperationErrors.Validation(nameof(command.DefaultLateThresholdMinutes), "ErrorLectureLateThreshold");
        }

        if (command.DefaultAllowedRadiusMeters < _options.MinimumAllowedRadiusMeters ||
            command.DefaultAllowedRadiusMeters > _options.MaximumAllowedRadiusMeters)
        {
            return OperationErrors.Validation(
                nameof(command.DefaultAllowedRadiusMeters),
                "ErrorLectureRadiusRange",
                _options.MinimumAllowedRadiusMeters,
                _options.MaximumAllowedRadiusMeters);
        }

        var section = await DbContext.Sections
            .AsNoTracking()
            .Include(item => item.Course)
            .Include(item => item.Enrollments)
            .FirstOrDefaultAsync(item => item.Id == command.SectionId, cancellationToken);

        if (section is null || !section.IsActive || section.Course is null || !section.Course.IsActive)
        {
            return OperationErrors.Dependency("ErrorActiveSectionAndCourseRequired");
        }

        var instructor = await DbContext.Instructors
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == command.InstructorId, cancellationToken);

        if (instructor is null || !instructor.IsActive)
        {
            return OperationErrors.Dependency("ErrorActiveInstructorRequired");
        }

        var instructorUserIsActive = await DbContext.Users
            .AsNoTracking()
            .AnyAsync(user =>
                user.Id == instructor.ApplicationUserId &&
                user.IsActive &&
                !user.IsDisabled,
                cancellationToken);

        if (!instructorUserIsActive)
        {
            return OperationErrors.Dependency("ErrorActiveInstructorAccountRequired");
        }

        var hasAssignment = await DbContext.InstructorAssignments.AnyAsync(assignment =>
            assignment.InstructorId == command.InstructorId &&
            assignment.SectionId == command.SectionId &&
            assignment.IsActive,
            cancellationToken);

        if (!hasAssignment)
        {
            return OperationErrors.Dependency("ErrorInstructorAssignmentRequired");
        }

        var classroom = await DbContext.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == command.ClassroomId, cancellationToken);

        if (classroom is null || !classroom.IsActive)
        {
            return OperationErrors.Dependency("ErrorActiveClassroomRequired");
        }

        var activeEnrollmentCount = section.Enrollments.Count(enrollment => enrollment.IsActive);
        if (classroom.Capacity < activeEnrollmentCount)
        {
            return OperationErrors.Dependency("ErrorClassroomCapacityLowerThanEnrollment", classroom.Capacity, activeEnrollmentCount);
        }

        return null;
    }

    private async Task<ScheduleConflictResult> FindConflictAsync(Guid? excludedScheduleId, LectureScheduleCommand command, CancellationToken cancellationToken)
    {
        var classroomConflict = await FindConflictByResourceAsync(
            ScheduleConflictType.Classroom,
            schedule => schedule.ClassroomId == command.ClassroomId,
            excludedScheduleId,
            command,
            cancellationToken);

        if (classroomConflict.HasConflict)
        {
            return classroomConflict;
        }

        var instructorConflict = await FindConflictByResourceAsync(
            ScheduleConflictType.Instructor,
            schedule => schedule.InstructorId == command.InstructorId,
            excludedScheduleId,
            command,
            cancellationToken);

        if (instructorConflict.HasConflict)
        {
            return instructorConflict;
        }

        return await FindConflictByResourceAsync(
            ScheduleConflictType.Section,
            schedule => schedule.SectionId == command.SectionId,
            excludedScheduleId,
            command,
            cancellationToken);
    }

    private async Task<ScheduleConflictResult> FindConflictByResourceAsync(
        ScheduleConflictType conflictType,
        Expression<Func<LectureSchedule, bool>> resourcePredicate,
        Guid? excludedScheduleId,
        LectureScheduleCommand command,
        CancellationToken cancellationToken)
    {
        var conflict = await BaseScheduleQuery()
            .AsNoTracking()
            .Where(resourcePredicate)
            .Where(schedule =>
                schedule.IsActive &&
                schedule.DayOfWeek == command.DayOfWeek &&
                schedule.EffectiveFrom <= command.EffectiveTo &&
                command.EffectiveFrom <= schedule.EffectiveTo &&
                schedule.StartTime < command.EndTime &&
                command.StartTime < schedule.EndTime &&
                (!excludedScheduleId.HasValue || schedule.Id != excludedScheduleId.Value))
            .OrderBy(schedule => schedule.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

        return conflict is null ? ScheduleConflictResult.None : ToConflictResult(conflictType, conflict);
    }

    private static ScheduleConflictResult ToConflictResult(ScheduleConflictType conflictType, LectureSchedule conflict)
        => new(
            true,
            conflictType,
            conflict.Id,
            conflict.Section?.Course?.NameEnglish,
            conflict.Section?.SectionNumber,
            conflict.Instructor?.NameEnglish,
            conflict.Classroom?.Code,
            conflict.DayOfWeek,
            conflict.StartTime,
            conflict.EndTime,
            conflict.EffectiveFrom,
            conflict.EffectiveTo,
            conflictType switch
            {
                ScheduleConflictType.Classroom => "ErrorScheduleClassroomConflict",
                ScheduleConflictType.Instructor => "ErrorScheduleInstructorConflict",
                ScheduleConflictType.Section => "ErrorScheduleSectionConflict",
                _ => "ErrorScheduleConflict"
            });

    private static OperationError ConflictError(ScheduleConflictResult conflict)
        => OperationErrors.Duplicate(nameof(LectureScheduleCommand.StartTime), conflict.LocalizedSafeMessage);
}
