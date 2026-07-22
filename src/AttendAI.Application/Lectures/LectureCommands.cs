namespace AttendAI.Application.Lectures;

public sealed class LectureScheduleCommand
{
    public Guid SectionId { get; set; }

    public Guid InstructorId { get; set; }

    public Guid ClassroomId { get; set; }

    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Sunday;

    public TimeOnly StartTime { get; set; } = new(9, 0);

    public TimeOnly EndTime { get; set; } = new(10, 0);

    public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    public DateOnly EffectiveTo { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(4));

    public int DefaultLateThresholdMinutes { get; set; } = 10;

    public int DefaultAllowedRadiusMeters { get; set; } = 50;

    public string? RowVersion { get; set; }
}

public sealed class StartLectureSessionCommand
{
    public Guid LectureScheduleId { get; set; }

    public DateOnly? SessionDate { get; set; }
}

public sealed class EndLectureSessionCommand
{
    public string? Reason { get; set; }

    public string? RowVersion { get; set; }
}
