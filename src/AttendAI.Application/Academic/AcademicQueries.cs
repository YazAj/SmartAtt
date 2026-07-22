using AttendAI.Application.Common.Models;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Academic;

public sealed class DepartmentQuery : PagedQuery
{
    public bool? IsActive { get; set; }
}

public sealed class StudentQuery : PagedQuery
{
    public Guid? DepartmentId { get; set; }

    public string? AcademicLevel { get; set; }

    public bool? IsActive { get; set; }
}

public sealed class InstructorQuery : PagedQuery
{
    public Guid? DepartmentId { get; set; }

    public string? AcademicTitle { get; set; }

    public bool? IsActive { get; set; }
}

public sealed class CourseQuery : PagedQuery
{
    public Guid? DepartmentId { get; set; }

    public bool? IsActive { get; set; }
}

public sealed class SectionQuery : PagedQuery
{
    public Guid? CourseId { get; set; }

    public string? AcademicYear { get; set; }

    public Semester? Semester { get; set; }

    public bool? IsActive { get; set; }
}

public sealed class ClassroomQuery : PagedQuery
{
    public bool? IsActive { get; set; }
}

public sealed class EnrollmentQuery : PagedQuery
{
    public Guid? StudentId { get; set; }

    public Guid? SectionId { get; set; }

    public Guid? CourseId { get; set; }

    public Semester? Semester { get; set; }

    public EnrollmentStatus? EnrollmentStatus { get; set; }
}

public sealed class InstructorAssignmentQuery : PagedQuery
{
    public Guid? InstructorId { get; set; }

    public Guid? SectionId { get; set; }

    public Guid? CourseId { get; set; }

    public bool? IsPrimary { get; set; }

    public bool? IsActive { get; set; }
}
