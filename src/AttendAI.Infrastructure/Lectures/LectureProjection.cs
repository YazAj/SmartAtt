using AttendAI.Application.Lectures;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Academic;

namespace AttendAI.Infrastructure.Lectures;

internal static class LectureProjection
{
    public static LectureScheduleDto ToDto(LectureSchedule schedule)
        => new(
            schedule.Id,
            schedule.SectionId,
            schedule.Section?.Course?.Code ?? string.Empty,
            schedule.Section?.Course?.NameEnglish ?? string.Empty,
            schedule.Section?.Course?.NameArabic ?? string.Empty,
            schedule.Section?.SectionNumber ?? string.Empty,
            schedule.Section?.AcademicYear ?? string.Empty,
            schedule.Section?.Semester ?? default,
            schedule.InstructorId,
            schedule.Instructor?.EmployeeNumber ?? string.Empty,
            schedule.Instructor?.NameEnglish ?? string.Empty,
            schedule.Instructor?.NameArabic ?? string.Empty,
            schedule.ClassroomId,
            schedule.Classroom?.Code ?? string.Empty,
            ClassroomLabelEnglish(schedule.Classroom),
            ClassroomLabelArabic(schedule.Classroom),
            schedule.DayOfWeek,
            schedule.StartTime,
            schedule.EndTime,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.DefaultLateThresholdMinutes,
            schedule.DefaultAllowedRadiusMeters,
            schedule.IsActive,
            schedule.CreatedAtUtc,
            schedule.UpdatedAtUtc,
            AcademicServiceBase.RowVersion(schedule.RowVersion));

    public static LectureSessionDto ToDto(
        LectureSession session,
        IApplicationTimeZoneService timeZoneService,
        bool includeSensitiveCodeState)
        => new(
            session.Id,
            session.LectureScheduleId,
            session.SectionId,
            session.Section?.Course?.Code ?? string.Empty,
            session.Section?.Course?.NameEnglish ?? string.Empty,
            session.Section?.Course?.NameArabic ?? string.Empty,
            session.Section?.SectionNumber ?? string.Empty,
            session.InstructorId,
            session.Instructor?.NameEnglish ?? string.Empty,
            session.Instructor?.NameArabic ?? string.Empty,
            session.ClassroomId,
            session.Classroom?.Code ?? string.Empty,
            session.SessionDate,
            session.ScheduledStartUtc,
            session.ScheduledEndUtc,
            timeZoneService.ConvertUtcToLocal(session.ScheduledStartUtc),
            timeZoneService.ConvertUtcToLocal(session.ScheduledEndUtc),
            session.ActualStartUtc,
            timeZoneService.ConvertUtcToLocal(session.ActualStartUtc),
            session.ActualEndUtc,
            session.ActualEndUtc.HasValue ? timeZoneService.ConvertUtcToLocal(session.ActualEndUtc.Value) : null,
            session.Status,
            session.SessionCodeExpiresAtUtc,
            session.SessionCodeExpiresAtUtc.HasValue ? timeZoneService.ConvertUtcToLocal(session.SessionCodeExpiresAtUtc.Value) : null,
            session.SessionCodeVersion,
            session.LateThresholdMinutes,
            session.AllowedRadiusMeters,
            session.EndReason,
            includeSensitiveCodeState && !string.IsNullOrWhiteSpace(session.ProtectedSessionCode),
            includeSensitiveCodeState && !string.IsNullOrWhiteSpace(session.SessionCodeHash),
            AcademicServiceBase.RowVersion(session.RowVersion));

    public static LectureSessionEventDto ToDto(LectureSessionEvent @event, IApplicationTimeZoneService timeZoneService)
        => new(
            @event.Id,
            @event.LectureSessionId,
            @event.EventType,
            @event.OccurredAtUtc,
            timeZoneService.ConvertUtcToLocal(@event.OccurredAtUtc),
            @event.PerformedByUserId,
            @event.PreviousStatus,
            @event.NewStatus,
            @event.SafeDescription);

    private static string ClassroomLabelEnglish(Classroom? classroom)
        => classroom is null ? string.Empty : $"{classroom.Code} - {classroom.BuildingNameEnglish} {classroom.RoomNumber}";

    private static string ClassroomLabelArabic(Classroom? classroom)
        => classroom is null ? string.Empty : $"{classroom.Code} - {classroom.BuildingNameArabic} {classroom.RoomNumber}";
}
