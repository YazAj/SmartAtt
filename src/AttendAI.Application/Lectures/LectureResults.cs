using AttendAI.Domain.Enums;

namespace AttendAI.Application.Lectures;

public sealed record ScheduleConflictResult(
    bool HasConflict,
    ScheduleConflictType ConflictType,
    Guid? ConflictingScheduleId,
    string? CourseName,
    string? SectionNumber,
    string? InstructorName,
    string? ClassroomCode,
    DayOfWeek? DayOfWeek,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    DateOnly? EffectiveFrom,
    DateOnly? EffectiveTo,
    string LocalizedSafeMessage)
{
    public static ScheduleConflictResult None { get; } = new(
        false,
        ScheduleConflictType.None,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        string.Empty);
}

public sealed record LectureSessionStartResult(
    Guid LectureSessionId,
    string PlainSessionCode,
    DateTimeOffset SessionCodeExpiresAtUtc,
    DateTimeOffset SessionCodeExpiresAtLocal);

public sealed record SessionCodeResult(
    string PlainSessionCode,
    DateTimeOffset SessionCodeExpiresAtUtc,
    DateTimeOffset SessionCodeExpiresAtLocal);
