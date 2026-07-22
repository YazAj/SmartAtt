namespace AttendAI.Domain.Academic;

public sealed class InstructorAssignment : AcademicEntity
{
    private InstructorAssignment()
    {
    }

    public InstructorAssignment(Guid instructorId, Guid sectionId, bool isPrimary)
    {
        if (instructorId == Guid.Empty)
        {
            throw new ArgumentException("Instructor is required.", nameof(instructorId));
        }

        if (sectionId == Guid.Empty)
        {
            throw new ArgumentException("Section is required.", nameof(sectionId));
        }

        InstructorId = instructorId;
        SectionId = sectionId;
        IsPrimary = isPrimary;
        AssignedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid InstructorId { get; private set; }

    public Instructor? Instructor { get; private set; }

    public Guid SectionId { get; private set; }

    public Section? Section { get; private set; }

    public bool IsPrimary { get; private set; }

    public DateTimeOffset AssignedAtUtc { get; private set; }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        Touch();
    }
}
