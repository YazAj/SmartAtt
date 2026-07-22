namespace AttendAI.Domain.Academic;

public sealed class Department : AcademicEntity
{
    private readonly List<Student> _students = [];
    private readonly List<Instructor> _instructors = [];
    private readonly List<Course> _courses = [];

    private Department()
    {
        Code = string.Empty;
        NameEnglish = string.Empty;
        NameArabic = string.Empty;
    }

    public Department(
        string code,
        string nameEnglish,
        string nameArabic,
        string? descriptionEnglish = null,
        string? descriptionArabic = null)
    {
        Update(code, nameEnglish, nameArabic, descriptionEnglish, descriptionArabic);
    }

    public string Code { get; private set; } = string.Empty;

    public string NameEnglish { get; private set; } = string.Empty;

    public string NameArabic { get; private set; } = string.Empty;

    public string? DescriptionEnglish { get; private set; }

    public string? DescriptionArabic { get; private set; }

    public IReadOnlyCollection<Student> Students => _students;

    public IReadOnlyCollection<Instructor> Instructors => _instructors;

    public IReadOnlyCollection<Course> Courses => _courses;

    public void Update(
        string code,
        string nameEnglish,
        string nameArabic,
        string? descriptionEnglish,
        string? descriptionArabic)
    {
        Code = NormalizeCode(code, nameof(Code), 32);
        NameEnglish = RequireText(nameEnglish, nameof(NameEnglish), 160);
        NameArabic = RequireText(nameArabic, nameof(NameArabic), 160);
        DescriptionEnglish = OptionalText(descriptionEnglish, 500);
        DescriptionArabic = OptionalText(descriptionArabic, 500);
        Touch();
    }
}
