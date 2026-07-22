using AttendAI.Domain.Enums;

namespace AttendAI.Domain.Academic;

public sealed class StudentEnrollment : AcademicEntity
{
    private StudentEnrollment()
    {
    }

    public StudentEnrollment(Guid studentId, Guid sectionId)
    {
        if (studentId == Guid.Empty)
        {
            throw new ArgumentException("Student is required.", nameof(studentId));
        }

        if (sectionId == Guid.Empty)
        {
            throw new ArgumentException("Section is required.", nameof(sectionId));
        }

        StudentId = studentId;
        SectionId = sectionId;
        EnrollmentStatus = EnrollmentStatus.Active;
        EnrolledAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid StudentId { get; private set; }

    public Student? Student { get; private set; }

    public Guid SectionId { get; private set; }

    public Section? Section { get; private set; }

    public EnrollmentStatus EnrollmentStatus { get; private set; }

    public DateTimeOffset EnrolledAtUtc { get; private set; }

    public void ChangeStatus(EnrollmentStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Enrollment status is not valid.");
        }

        EnrollmentStatus = status;
        IsActive = status == EnrollmentStatus.Active;
        Touch();
    }
}
