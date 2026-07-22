using AttendAI.Domain.Enums;

namespace AttendAI.Application.Academic;

public sealed class DepartmentCommand
{
    public string Code { get; set; } = string.Empty;

    public string NameEnglish { get; set; } = string.Empty;

    public string NameArabic { get; set; } = string.Empty;

    public string? DescriptionEnglish { get; set; }

    public string? DescriptionArabic { get; set; }

    public string? RowVersion { get; set; }
}

public sealed class StudentAccountCommand
{
    public string Email { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;

    public string ConfirmTemporaryPassword { get; set; } = string.Empty;

    public string StudentNumber { get; set; } = string.Empty;

    public string NameEnglish { get; set; } = string.Empty;

    public string NameArabic { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public int EnrollmentYear { get; set; } = DateTime.UtcNow.Year;

    public string AcademicLevel { get; set; } = string.Empty;
}

public sealed class StudentProfileCommand
{
    public string StudentNumber { get; set; } = string.Empty;

    public string NameEnglish { get; set; } = string.Empty;

    public string NameArabic { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public int EnrollmentYear { get; set; } = DateTime.UtcNow.Year;

    public string AcademicLevel { get; set; } = string.Empty;

    public string? RowVersion { get; set; }
}

public sealed class InstructorAccountCommand
{
    public string Email { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;

    public string ConfirmTemporaryPassword { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string NameEnglish { get; set; } = string.Empty;

    public string NameArabic { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public string AcademicTitle { get; set; } = string.Empty;
}

public sealed class InstructorProfileCommand
{
    public string EmployeeNumber { get; set; } = string.Empty;

    public string NameEnglish { get; set; } = string.Empty;

    public string NameArabic { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public string AcademicTitle { get; set; } = string.Empty;

    public string? RowVersion { get; set; }
}

public sealed class CourseCommand
{
    public string Code { get; set; } = string.Empty;

    public string NameEnglish { get; set; } = string.Empty;

    public string NameArabic { get; set; } = string.Empty;

    public string? DescriptionEnglish { get; set; }

    public string? DescriptionArabic { get; set; }

    public int CreditHours { get; set; } = 3;

    public Guid DepartmentId { get; set; }

    public string? RowVersion { get; set; }
}

public sealed class SectionCommand
{
    public Guid CourseId { get; set; }

    public string SectionNumber { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public Semester Semester { get; set; } = Semester.First;

    public int Capacity { get; set; } = 30;

    public string? RowVersion { get; set; }
}

public sealed class ClassroomCommand
{
    public string Code { get; set; } = string.Empty;

    public string BuildingNameEnglish { get; set; } = string.Empty;

    public string BuildingNameArabic { get; set; } = string.Empty;

    public string RoomNumber { get; set; } = string.Empty;

    public int Capacity { get; set; } = 30;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? RowVersion { get; set; }
}

public sealed class EnrollmentCommand
{
    public Guid StudentId { get; set; }

    public Guid SectionId { get; set; }
}

public sealed class EnrollmentStatusCommand
{
    public EnrollmentStatus EnrollmentStatus { get; set; } = EnrollmentStatus.Active;

    public string? RowVersion { get; set; }
}

public sealed class InstructorAssignmentCommand
{
    public Guid InstructorId { get; set; }

    public Guid SectionId { get; set; }

    public bool IsPrimary { get; set; }
}

public sealed class InstructorAssignmentPrimaryCommand
{
    public bool IsPrimary { get; set; }

    public string? RowVersion { get; set; }
}

public sealed class TemporaryPasswordCommand
{
    public string TemporaryPassword { get; set; } = string.Empty;

    public string ConfirmTemporaryPassword { get; set; } = string.Empty;
}
