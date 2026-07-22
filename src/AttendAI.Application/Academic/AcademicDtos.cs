using AttendAI.Domain.Enums;

namespace AttendAI.Application.Academic;

public sealed record DepartmentDto(
    Guid Id,
    string Code,
    string NameEnglish,
    string NameArabic,
    string? DescriptionEnglish,
    string? DescriptionArabic,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record StudentDto(
    Guid Id,
    string ApplicationUserId,
    string Email,
    string StudentNumber,
    string NameEnglish,
    string NameArabic,
    Guid DepartmentId,
    string DepartmentNameEnglish,
    string DepartmentNameArabic,
    int EnrollmentYear,
    string AcademicLevel,
    bool IsActive,
    bool MustChangePassword,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record InstructorDto(
    Guid Id,
    string ApplicationUserId,
    string Email,
    string EmployeeNumber,
    string NameEnglish,
    string NameArabic,
    Guid DepartmentId,
    string DepartmentNameEnglish,
    string DepartmentNameArabic,
    string AcademicTitle,
    bool IsActive,
    bool MustChangePassword,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record CourseDto(
    Guid Id,
    string Code,
    string NameEnglish,
    string NameArabic,
    string? DescriptionEnglish,
    string? DescriptionArabic,
    int CreditHours,
    Guid DepartmentId,
    string DepartmentNameEnglish,
    string DepartmentNameArabic,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record SectionDto(
    Guid Id,
    Guid CourseId,
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string SectionNumber,
    string AcademicYear,
    Semester Semester,
    int Capacity,
    int ActiveEnrollmentCount,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record ClassroomDto(
    Guid Id,
    string Code,
    string BuildingNameEnglish,
    string BuildingNameArabic,
    string RoomNumber,
    int Capacity,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record StudentEnrollmentDto(
    Guid Id,
    Guid StudentId,
    string StudentNumber,
    string StudentNameEnglish,
    string StudentNameArabic,
    Guid SectionId,
    string SectionLabel,
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string AcademicYear,
    Semester Semester,
    EnrollmentStatus EnrollmentStatus,
    DateTimeOffset EnrolledAtUtc,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);

public sealed record InstructorAssignmentDto(
    Guid Id,
    Guid InstructorId,
    string EmployeeNumber,
    string InstructorNameEnglish,
    string InstructorNameArabic,
    Guid SectionId,
    string SectionLabel,
    string CourseCode,
    string CourseNameEnglish,
    string CourseNameArabic,
    string AcademicYear,
    Semester Semester,
    bool IsPrimary,
    DateTimeOffset AssignedAtUtc,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    string RowVersion);
