using AttendAI.Application.Academic;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

internal static class AcademicProjection
{
    public static DepartmentDto ToDto(Department department)
        => new(
            department.Id,
            department.Code,
            department.NameEnglish,
            department.NameArabic,
            department.DescriptionEnglish,
            department.DescriptionArabic,
            department.IsActive,
            department.CreatedAtUtc,
            department.UpdatedAtUtc,
            AcademicServiceBase.RowVersion(department.RowVersion));

    public static CourseDto ToDto(Course course)
        => new(
            course.Id,
            course.Code,
            course.NameEnglish,
            course.NameArabic,
            course.DescriptionEnglish,
            course.DescriptionArabic,
            course.CreditHours,
            course.DepartmentId,
            course.Department?.NameEnglish ?? string.Empty,
            course.Department?.NameArabic ?? string.Empty,
            course.IsActive,
            course.CreatedAtUtc,
            course.UpdatedAtUtc,
            AcademicServiceBase.RowVersion(course.RowVersion));

    public static SectionDto ToDto(Section section)
        => new(
            section.Id,
            section.CourseId,
            section.Course?.Code ?? string.Empty,
            section.Course?.NameEnglish ?? string.Empty,
            section.Course?.NameArabic ?? string.Empty,
            section.SectionNumber,
            section.AcademicYear,
            section.Semester,
            section.Capacity,
            section.Enrollments.Count(enrollment => enrollment.IsActive),
            section.IsActive,
            section.CreatedAtUtc,
            section.UpdatedAtUtc,
            AcademicServiceBase.RowVersion(section.RowVersion));

    public static ClassroomDto ToDto(Classroom classroom)
        => new(
            classroom.Id,
            classroom.Code,
            classroom.BuildingNameEnglish,
            classroom.BuildingNameArabic,
            classroom.RoomNumber,
            classroom.Capacity,
            classroom.Latitude,
            classroom.Longitude,
            classroom.IsActive,
            classroom.CreatedAtUtc,
            classroom.UpdatedAtUtc,
            AcademicServiceBase.RowVersion(classroom.RowVersion));

    public static async Task<StudentDto?> StudentToDtoAsync(
        IQueryable<Student> students,
        IQueryable<ApplicationUser> users,
        Guid id,
        CancellationToken cancellationToken)
        => await (
            from student in students
            join user in users on student.ApplicationUserId equals user.Id
            where student.Id == id
            select new StudentDto(
                student.Id,
                student.ApplicationUserId,
                user.Email ?? string.Empty,
                student.StudentNumber,
                student.NameEnglish,
                student.NameArabic,
                student.DepartmentId,
                student.Department!.NameEnglish,
                student.Department.NameArabic,
                student.EnrollmentYear,
                student.AcademicLevel,
                student.IsActive,
                EF.Property<bool>(user, "MustChangePassword"),
                student.CreatedAtUtc,
                student.UpdatedAtUtc,
                AcademicServiceBase.RowVersion(student.RowVersion))).FirstOrDefaultAsync(cancellationToken);
}
