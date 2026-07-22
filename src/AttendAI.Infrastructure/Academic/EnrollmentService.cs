using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class EnrollmentService : AcademicServiceBase, IEnrollmentService
{
    public EnrollmentService(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<PagedResult<StudentEnrollmentDto>> GetPagedAsync(EnrollmentQuery query, CancellationToken cancellationToken = default)
    {
        var enrollments = DbContext.StudentEnrollments
            .Include(enrollment => enrollment.Student)
            .Include(enrollment => enrollment.Section)
                .ThenInclude(section => section!.Course)
            .AsNoTracking();

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            enrollments = enrollments.Where(enrollment =>
                enrollment.Student!.StudentNumber.Contains(search) ||
                enrollment.Student.NameEnglish.Contains(search) ||
                enrollment.Student.NameArabic.Contains(search) ||
                enrollment.Section!.SectionNumber.Contains(search) ||
                enrollment.Section.Course!.Code.Contains(search));
        }

        if (query.StudentId.HasValue)
        {
            enrollments = enrollments.Where(enrollment => enrollment.StudentId == query.StudentId.Value);
        }

        if (query.SectionId.HasValue)
        {
            enrollments = enrollments.Where(enrollment => enrollment.SectionId == query.SectionId.Value);
        }

        if (query.CourseId.HasValue)
        {
            enrollments = enrollments.Where(enrollment => enrollment.Section!.CourseId == query.CourseId.Value);
        }

        if (query.Semester.HasValue)
        {
            enrollments = enrollments.Where(enrollment => enrollment.Section!.Semester == query.Semester.Value);
        }

        if (query.EnrollmentStatus.HasValue)
        {
            enrollments = enrollments.Where(enrollment => enrollment.EnrollmentStatus == query.EnrollmentStatus.Value);
        }

        enrollments = query.SortBy?.ToLowerInvariant() switch
        {
            "student" => query.SortDescending ? enrollments.OrderByDescending(e => e.Student!.StudentNumber) : enrollments.OrderBy(e => e.Student!.StudentNumber),
            "course" => query.SortDescending ? enrollments.OrderByDescending(e => e.Section!.Course!.Code) : enrollments.OrderBy(e => e.Section!.Course!.Code),
            "semester" => query.SortDescending ? enrollments.OrderByDescending(e => e.Section!.Semester) : enrollments.OrderBy(e => e.Section!.Semester),
            "status" => query.SortDescending ? enrollments.OrderByDescending(e => e.EnrollmentStatus) : enrollments.OrderBy(e => e.EnrollmentStatus),
            _ => query.SortDescending ? enrollments.OrderByDescending(e => e.EnrolledAtUtc) : enrollments.OrderBy(e => e.EnrolledAtUtc)
        };

        var total = await enrollments.CountAsync(cancellationToken);
        var rows = await ApplyPaging(enrollments, query).ToListAsync(cancellationToken);
        var items = rows.Select(ToDto).ToList();
        return new PagedResult<StudentEnrollmentDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<StudentEnrollmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var enrollment = await DbContext.StudentEnrollments
            .Include(item => item.Student)
            .Include(item => item.Section)
                .ThenInclude(section => section!.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return enrollment is null ? null : ToDto(enrollment);
    }

    public async Task<OperationResult<Guid>> CreateAsync(EnrollmentCommand command, CancellationToken cancellationToken = default)
    {
        var student = await DbContext.Students.AsNoTracking().FirstOrDefaultAsync(item => item.Id == command.StudentId, cancellationToken);
        if (student is null || !student.IsActive)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorActiveStudentRequired"));
        }

        var section = await DbContext.Sections.AsNoTracking().FirstOrDefaultAsync(item => item.Id == command.SectionId, cancellationToken);
        if (section is null || !section.IsActive)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorActiveSectionRequired"));
        }

        if (await DbContext.StudentEnrollments.AnyAsync(item => item.StudentId == command.StudentId && item.SectionId == command.SectionId, cancellationToken))
        {
            return OperationResult<Guid>.Failure(OperationErrors.Duplicate(nameof(command.SectionId), "ErrorDuplicateEnrollment"));
        }

        var activeCount = await DbContext.StudentEnrollments.CountAsync(item =>
            item.SectionId == command.SectionId &&
            item.IsActive &&
            item.EnrollmentStatus == EnrollmentStatus.Active,
            cancellationToken);

        if (activeCount >= section.Capacity)
        {
            return OperationResult<Guid>.Failure(OperationErrors.Dependency("ErrorSectionCapacityReached"));
        }

        var enrollment = new StudentEnrollment(command.StudentId, command.SectionId);
        DbContext.StudentEnrollments.Add(enrollment);
        return await SaveCreatedAsync(enrollment.Id, cancellationToken);
    }

    public async Task<OperationResult> ChangeStatusAsync(Guid id, EnrollmentStatusCommand command, CancellationToken cancellationToken = default)
    {
        var enrollment = await DbContext.StudentEnrollments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (enrollment is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (command.EnrollmentStatus == EnrollmentStatus.Active)
        {
            var section = await DbContext.Sections.AsNoTracking().FirstOrDefaultAsync(item => item.Id == enrollment.SectionId, cancellationToken);
            if (section is null || !section.IsActive)
            {
                return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveSectionRequired"));
            }

            var activeCount = await DbContext.StudentEnrollments.CountAsync(item =>
                item.SectionId == enrollment.SectionId &&
                item.Id != id &&
                item.IsActive &&
                item.EnrollmentStatus == EnrollmentStatus.Active,
                cancellationToken);

            if (activeCount >= section.Capacity)
            {
                return OperationResult.Failure(OperationErrors.Dependency("ErrorSectionCapacityReached"));
            }
        }

        SetOriginalRowVersion(enrollment, command.RowVersion);
        enrollment.ChangeStatus(command.EnrollmentStatus);
        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var enrollment = await DbContext.StudentEnrollments.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (enrollment is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        enrollment.ChangeStatus(EnrollmentStatus.Cancelled);
        enrollment.Deactivate();
        return await SaveChangesAsync(cancellationToken);
    }

    private static StudentEnrollmentDto ToDto(StudentEnrollment enrollment)
        => new(
            enrollment.Id,
            enrollment.StudentId,
            enrollment.Student?.StudentNumber ?? string.Empty,
            enrollment.Student?.NameEnglish ?? string.Empty,
            enrollment.Student?.NameArabic ?? string.Empty,
            enrollment.SectionId,
            $"{enrollment.Section?.Course?.Code}-{enrollment.Section?.SectionNumber}",
            enrollment.Section?.Course?.Code ?? string.Empty,
            enrollment.Section?.Course?.NameEnglish ?? string.Empty,
            enrollment.Section?.Course?.NameArabic ?? string.Empty,
            enrollment.Section?.AcademicYear ?? string.Empty,
            enrollment.Section?.Semester ?? Semester.First,
            enrollment.EnrollmentStatus,
            enrollment.EnrolledAtUtc,
            enrollment.IsActive,
            enrollment.CreatedAtUtc,
            enrollment.UpdatedAtUtc,
            RowVersion(enrollment.RowVersion));
}
