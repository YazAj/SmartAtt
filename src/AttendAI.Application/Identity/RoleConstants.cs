namespace AttendAI.Application.Identity;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Instructor = "Instructor";
    public const string Student = "Student";

    public static readonly IReadOnlyCollection<string> All = [Admin, Instructor, Student];
}
