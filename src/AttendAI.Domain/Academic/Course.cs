namespace AttendAI.Domain.Academic;

public sealed class Course : AcademicEntity
{
    private readonly List<Section> _sections = [];

    private Course()
    {
        Code = string.Empty;
        NameEnglish = string.Empty;
        NameArabic = string.Empty;
    }

    public Course(
        string code,
        string nameEnglish,
        string nameArabic,
        Guid departmentId,
        int creditHours,
        string? descriptionEnglish = null,
        string? descriptionArabic = null)
    {
        Update(code, nameEnglish, nameArabic, departmentId, creditHours, descriptionEnglish, descriptionArabic);
    }

    public string Code { get; private set; } = string.Empty;

    public string NameEnglish { get; private set; } = string.Empty;

    public string NameArabic { get; private set; } = string.Empty;

    public string? DescriptionEnglish { get; private set; }

    public string? DescriptionArabic { get; private set; }

    public int CreditHours { get; private set; }

    public Guid DepartmentId { get; private set; }

    public Department? Department { get; private set; }

    public IReadOnlyCollection<Section> Sections => _sections;

    public void Update(
        string code,
        string nameEnglish,
        string nameArabic,
        Guid departmentId,
        int creditHours,
        string? descriptionEnglish,
        string? descriptionArabic)
    {
        if (departmentId == Guid.Empty)
        {
            throw new ArgumentException("Department is required.", nameof(departmentId));
        }

        if (creditHours is < 1 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(creditHours), "Credit hours must be between 1 and 6.");
        }

        Code = NormalizeCode(code, nameof(Code), 32);
        NameEnglish = RequireText(nameEnglish, nameof(NameEnglish), 180);
        NameArabic = RequireText(nameArabic, nameof(NameArabic), 180);
        DepartmentId = departmentId;
        CreditHours = creditHours;
        DescriptionEnglish = OptionalText(descriptionEnglish, 500);
        DescriptionArabic = OptionalText(descriptionArabic, 500);
        Touch();
    }
}
