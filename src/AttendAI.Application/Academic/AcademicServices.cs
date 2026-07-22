using AttendAI.Application.Common.Models;

namespace AttendAI.Application.Academic;

public interface IDepartmentService
{
    Task<PagedResult<DepartmentDto>> GetPagedAsync(DepartmentQuery query, CancellationToken cancellationToken = default);

    Task<DepartmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(DepartmentCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateAsync(Guid id, DepartmentCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IStudentService
{
    Task<PagedResult<StudentDto>> GetPagedAsync(StudentQuery query, CancellationToken cancellationToken = default);

    Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<StudentDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAccountAsync(StudentAccountCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateProfileAsync(Guid id, StudentProfileCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> ResetTemporaryPasswordAsync(Guid id, TemporaryPasswordCommand command, CancellationToken cancellationToken = default);
}

public interface IInstructorService
{
    Task<PagedResult<InstructorDto>> GetPagedAsync(InstructorQuery query, CancellationToken cancellationToken = default);

    Task<InstructorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<InstructorDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAccountAsync(InstructorAccountCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateProfileAsync(Guid id, InstructorProfileCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> ResetTemporaryPasswordAsync(Guid id, TemporaryPasswordCommand command, CancellationToken cancellationToken = default);
}

public interface ICourseService
{
    Task<PagedResult<CourseDto>> GetPagedAsync(CourseQuery query, CancellationToken cancellationToken = default);

    Task<CourseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(CourseCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateAsync(Guid id, CourseCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ISectionService
{
    Task<PagedResult<SectionDto>> GetPagedAsync(SectionQuery query, CancellationToken cancellationToken = default);

    Task<SectionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(SectionCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateAsync(Guid id, SectionCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IClassroomService
{
    Task<PagedResult<ClassroomDto>> GetPagedAsync(ClassroomQuery query, CancellationToken cancellationToken = default);

    Task<ClassroomDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(ClassroomCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> UpdateAsync(Guid id, ClassroomCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IEnrollmentService
{
    Task<PagedResult<StudentEnrollmentDto>> GetPagedAsync(EnrollmentQuery query, CancellationToken cancellationToken = default);

    Task<StudentEnrollmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(EnrollmentCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> ChangeStatusAsync(Guid id, EnrollmentStatusCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IInstructorAssignmentService
{
    Task<PagedResult<InstructorAssignmentDto>> GetPagedAsync(InstructorAssignmentQuery query, CancellationToken cancellationToken = default);

    Task<InstructorAssignmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OperationResult<Guid>> CreateAsync(InstructorAssignmentCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> SetPrimaryAsync(Guid id, InstructorAssignmentPrimaryCommand command, CancellationToken cancellationToken = default);

    Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IAcademicLookupService
{
    Task<IReadOnlyList<LookupItem>> GetActiveDepartmentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem>> GetActiveCoursesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem>> GetActiveSectionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem>> GetActiveStudentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItem>> GetActiveInstructorsAsync(CancellationToken cancellationToken = default);
}
