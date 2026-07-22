namespace AttendAI.Domain.Academic;

public sealed class LectureSchedule : AcademicEntity
{
    private readonly List<LectureSession> _sessions = [];

    private LectureSchedule()
    {
    }

    public LectureSchedule(
        Guid sectionId,
        Guid instructorId,
        Guid classroomId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        DateOnly effectiveFrom,
        DateOnly effectiveTo,
        int defaultLateThresholdMinutes,
        int defaultAllowedRadiusMeters)
    {
        Update(
            sectionId,
            instructorId,
            classroomId,
            dayOfWeek,
            startTime,
            endTime,
            effectiveFrom,
            effectiveTo,
            defaultLateThresholdMinutes,
            defaultAllowedRadiusMeters);
    }

    public Guid SectionId { get; private set; }

    public Section? Section { get; private set; }

    public Guid InstructorId { get; private set; }

    public Instructor? Instructor { get; private set; }

    public Guid ClassroomId { get; private set; }

    public Classroom? Classroom { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public DateOnly EffectiveFrom { get; private set; }

    public DateOnly EffectiveTo { get; private set; }

    public int DefaultLateThresholdMinutes { get; private set; }

    public int DefaultAllowedRadiusMeters { get; private set; }

    public IReadOnlyCollection<LectureSession> Sessions => _sessions;

    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    public void Update(
        Guid sectionId,
        Guid instructorId,
        Guid classroomId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        DateOnly effectiveFrom,
        DateOnly effectiveTo,
        int defaultLateThresholdMinutes,
        int defaultAllowedRadiusMeters)
    {
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

        if (!Enum.IsDefined(dayOfWeek))
        {
            throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "Day of week is not valid.");
        }

        if (startTime >= endTime)
        {
            throw new ArgumentException("Start time must be before end time.", nameof(startTime));
        }

        if (effectiveFrom > effectiveTo)
        {
            throw new ArgumentException("Effective-from date must be on or before effective-to date.", nameof(effectiveFrom));
        }

        var durationMinutes = (int)(endTime - startTime).TotalMinutes;
        if (defaultLateThresholdMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultLateThresholdMinutes), "Late threshold cannot be negative.");
        }

        if (defaultLateThresholdMinutes >= durationMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultLateThresholdMinutes), "Late threshold must be shorter than the lecture duration.");
        }

        if (defaultAllowedRadiusMeters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultAllowedRadiusMeters), "Allowed radius cannot be negative.");
        }

        SectionId = sectionId;
        InstructorId = instructorId;
        ClassroomId = classroomId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        DefaultLateThresholdMinutes = defaultLateThresholdMinutes;
        DefaultAllowedRadiusMeters = defaultAllowedRadiusMeters;
        Touch();
    }

    public bool IsEffectiveOn(DateOnly date)
        => IsActive && date >= EffectiveFrom && date <= EffectiveTo && date.DayOfWeek == DayOfWeek;
}
