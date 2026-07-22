namespace AttendAI.Domain.Academic;

public sealed class Instructor : AcademicEntity
{
    private readonly List<InstructorAssignment> _assignments = [];

    private Instructor()
    {
        ApplicationUserId = string.Empty;
        EmployeeNumber = string.Empty;
        NameEnglish = string.Empty;
        NameArabic = string.Empty;
        AcademicTitle = string.Empty;
    }

    public Instructor(
        string applicationUserId,
        string employeeNumber,
        string nameEnglish,
        string nameArabic,
        Guid departmentId,
        string academicTitle)
    {
        ApplicationUserId = RequireText(applicationUserId, nameof(ApplicationUserId), 450);
        UpdateProfile(employeeNumber, nameEnglish, nameArabic, departmentId, academicTitle);
    }

    public string ApplicationUserId { get; private set; }

    public string EmployeeNumber { get; private set; } = string.Empty;

    public string NameEnglish { get; private set; } = string.Empty;

    public string NameArabic { get; private set; } = string.Empty;

    public Guid DepartmentId { get; private set; }

    public Department? Department { get; private set; }

    public string AcademicTitle { get; private set; } = string.Empty;

    public IReadOnlyCollection<InstructorAssignment> Assignments => _assignments;

    public void UpdateProfile(
        string employeeNumber,
        string nameEnglish,
        string nameArabic,
        Guid departmentId,
        string academicTitle)
    {
        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Department is required.", nameof(departmentId));
        }

        EmployeeNumber = RequireText(employeeNumber, nameof(EmployeeNumber), 32).ToUpperInvariant();
        NameEnglish = RequireText(nameEnglish, nameof(NameEnglish), 160);
        NameArabic = RequireText(nameArabic, nameof(NameArabic), 160);
        DepartmentId = departmentId;
        AcademicTitle = RequireText(academicTitle, nameof(AcademicTitle), 120);
        Touch();
    }
}
