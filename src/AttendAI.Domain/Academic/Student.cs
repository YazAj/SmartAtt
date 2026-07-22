namespace AttendAI.Domain.Academic;

public sealed class Student : AcademicEntity
{
    private readonly List<StudentEnrollment> _enrollments = [];

    private Student()
    {
        ApplicationUserId = string.Empty;
        StudentNumber = string.Empty;
        NameEnglish = string.Empty;
        NameArabic = string.Empty;
        AcademicLevel = string.Empty;
    }

    public Student(
        string applicationUserId,
        string studentNumber,
        string nameEnglish,
        string nameArabic,
        Guid departmentId,
        int enrollmentYear,
        string academicLevel)
    {
        ApplicationUserId = RequireText(applicationUserId, nameof(ApplicationUserId), 450);
        UpdateProfile(studentNumber, nameEnglish, nameArabic, departmentId, enrollmentYear, academicLevel);
    }

    public string ApplicationUserId { get; private set; }

    public string StudentNumber { get; private set; } = string.Empty;

    public string NameEnglish { get; private set; } = string.Empty;

    public string NameArabic { get; private set; } = string.Empty;

    public Guid DepartmentId { get; private set; }

    public Department? Department { get; private set; }

    public int EnrollmentYear { get; private set; }

    public string AcademicLevel { get; private set; } = string.Empty;

    public IReadOnlyCollection<StudentEnrollment> Enrollments => _enrollments;

    public void UpdateProfile(
        string studentNumber,
        string nameEnglish,
        string nameArabic,
        Guid departmentId,
        int enrollmentYear,
        string academicLevel)
    {
        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Department is required.", nameof(departmentId));
        }

        if (enrollmentYear is < 2000 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(enrollmentYear), "Enrollment year must be between 2000 and 2100.");
        }

        StudentNumber = RequireText(studentNumber, nameof(StudentNumber), 32).ToUpperInvariant();
        NameEnglish = RequireText(nameEnglish, nameof(NameEnglish), 160);
        NameArabic = RequireText(nameArabic, nameof(NameArabic), 160);
        DepartmentId = departmentId;
        EnrollmentYear = enrollmentYear;
        AcademicLevel = RequireText(academicLevel, nameof(AcademicLevel), 60);
        Touch();
    }
}
