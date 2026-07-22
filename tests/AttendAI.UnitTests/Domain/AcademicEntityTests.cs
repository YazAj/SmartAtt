using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;

namespace AttendAI.UnitTests.Domain;

public sealed class AcademicEntityTests
{
    [Fact]
    public void Department_normalizes_code_to_uppercase()
    {
        var department = new Department(" cs ", "Computer Science", "علوم الحاسوب");

        Assert.Equal("CS", department.Code);
    }

    [Fact]
    public void Department_requires_english_and_arabic_names()
    {
        Assert.Throws<ArgumentException>(() => new Department("CS", "", "علوم الحاسوب"));
        Assert.Throws<ArgumentException>(() => new Department("CS", "Computer Science", ""));
    }

    [Fact]
    public void Course_rejects_invalid_credit_hours()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Course("CS101", "Intro", "مقدمة", Guid.NewGuid(), 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Course("CS101", "Intro", "مقدمة", Guid.NewGuid(), 7));
    }

    [Fact]
    public void Section_rejects_non_positive_capacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Section(Guid.NewGuid(), "1", "2026/2027", Semester.First, 0));
    }

    [Fact]
    public void Classroom_requires_coordinate_pair()
    {
        Assert.Throws<ArgumentException>(() => new Classroom("B101", "Engineering", "الهندسة", "101", 40, 31.9m, null));
    }

    [Fact]
    public void Classroom_validates_coordinate_ranges()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Classroom("B101", "Engineering", "الهندسة", "101", 40, 91m, 35m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Classroom("B101", "Engineering", "الهندسة", "101", 40, 31m, 181m));
    }

    [Fact]
    public void Student_requires_valid_enrollment_year()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Student("user-id", "S1", "Student", "طالب", Guid.NewGuid(), 1999, "First"));
    }

    [Fact]
    public void Student_enrollment_defaults_to_active_status()
    {
        var enrollment = new StudentEnrollment(Guid.NewGuid(), Guid.NewGuid());

        Assert.True(enrollment.IsActive);
        Assert.Equal(EnrollmentStatus.Active, enrollment.EnrollmentStatus);
    }

    [Fact]
    public void Instructor_assignment_preserves_primary_flag()
    {
        var assignment = new InstructorAssignment(Guid.NewGuid(), Guid.NewGuid(), isPrimary: true);

        Assert.True(assignment.IsPrimary);
    }
}
