using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class Section : AcademicEntity
{
    private readonly List<StudentEnrollment> _enrollments = [];
    private readonly List<InstructorAssignment> _instructorAssignments = [];

    private Section()
    {
        SectionNumber = string.Empty;
        AcademicYear = string.Empty;
    }

    public Section(
        Guid courseId,
        string sectionNumber,
        string academicYear,
        Semester semester,
        int capacity)
    {
        Update(courseId, sectionNumber, academicYear, semester, capacity);
    }

    public Guid CourseId { get; private set; }

    public Course? Course { get; private set; }

    public string SectionNumber { get; private set; } = string.Empty;

    public string AcademicYear { get; private set; } = string.Empty;

    public Semester Semester { get; private set; }

    public int Capacity { get; private set; }

    public IReadOnlyCollection<StudentEnrollment> Enrollments => _enrollments;

    public IReadOnlyCollection<InstructorAssignment> InstructorAssignments => _instructorAssignments;

    public void Update(Guid courseId, string sectionNumber, string academicYear, Semester semester, int capacity)
    {
        if (courseId == Guid.Empty)
        {
            throw new ArgumentException("Course is required.", nameof(courseId));
        }

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        if (!Enum.IsDefined(semester))
        {
            throw new ArgumentOutOfRangeException(nameof(semester), "Semester is not valid.");
        }

        CourseId = courseId;
        SectionNumber = RequireText(sectionNumber, nameof(SectionNumber), 32).ToUpperInvariant();
        AcademicYear = RequireText(academicYear, nameof(AcademicYear), 20);
        Semester = semester;
        Capacity = capacity;
        Touch();
    }
}
