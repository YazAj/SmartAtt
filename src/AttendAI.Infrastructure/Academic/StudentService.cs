using System.ComponentModel.DataAnnotations;
using AttendAI.Application.Academic;
using AttendAI.Application.Common.Models;
using AttendAI.Application.Identity;
using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Academic;

public sealed class StudentService : AcademicServiceBase, IStudentService
{
    private static readonly EmailAddressAttribute EmailValidator = new();
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
        : base(dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PagedResult<StudentDto>> GetPagedAsync(StudentQuery query, CancellationToken cancellationToken = default)
    {
        var students =
            from student in DbContext.Students.Include(student => student.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            select new { Student = student, User = user };

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            students = students.Where(item =>
                item.Student.StudentNumber.Contains(search) ||
                item.Student.NameEnglish.Contains(search) ||
                item.Student.NameArabic.Contains(search) ||
                item.User.Email!.Contains(search));
        }

        if (query.DepartmentId.HasValue)
        {
            students = students.Where(item => item.Student.DepartmentId == query.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.AcademicLevel))
        {
            students = students.Where(item => item.Student.AcademicLevel == query.AcademicLevel);
        }

        if (query.IsActive.HasValue)
        {
            students = students.Where(item => item.Student.IsActive == query.IsActive.Value);
        }

        students = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending ? students.OrderByDescending(item => item.Student.NameEnglish) : students.OrderBy(item => item.Student.NameEnglish),
            "email" => query.SortDescending ? students.OrderByDescending(item => item.User.Email) : students.OrderBy(item => item.User.Email),
            "department" => query.SortDescending ? students.OrderByDescending(item => item.Student.Department!.NameEnglish) : students.OrderBy(item => item.Student.Department!.NameEnglish),
            "level" => query.SortDescending ? students.OrderByDescending(item => item.Student.AcademicLevel) : students.OrderBy(item => item.Student.AcademicLevel),
            "status" => query.SortDescending ? students.OrderByDescending(item => item.Student.IsActive) : students.OrderBy(item => item.Student.IsActive),
            _ => query.SortDescending ? students.OrderByDescending(item => item.Student.StudentNumber) : students.OrderBy(item => item.Student.StudentNumber)
        };

        var total = await students.CountAsync(cancellationToken);
        var rows = await ApplyPaging(students, query).ToListAsync(cancellationToken);
        var items = rows.Select(item => ToDto(item.Student, item.User)).ToList();
        return new PagedResult<StudentDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await (
            from student in DbContext.Students.Include(student => student.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.Id == id
            select new { Student = student, User = user }).FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToDto(row.Student, row.User);
    }

    public async Task<StudentDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var row = await (
            from student in DbContext.Students.Include(student => student.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on student.ApplicationUserId equals user.Id
            where student.ApplicationUserId == userId
            select new { Student = student, User = user }).FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToDto(row.Student, row.User);
    }

    public async Task<OperationResult<Guid>> CreateAccountAsync(StudentAccountCommand command, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateCreateAsync(command, cancellationToken);
        if (!validation.Succeeded)
        {
            return OperationResult<Guid>.Failure(validation.Errors.ToArray());
        }

        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);

        var email = NormalizeText(command.Email);
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = command.NameEnglish.Trim(),
            IsActive = true,
            IsDisabled = false,
            MustChangePassword = true
        };

        var createUserResult = await _userManager.CreateAsync(user, command.TemporaryPassword);
        if (!createUserResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return IdentityFailure<Guid>(createUserResult);
        }

        await EnsureRoleAsync(RoleConstants.Student);
        var roleResult = await _userManager.AddToRoleAsync(user, RoleConstants.Student);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return IdentityFailure<Guid>(roleResult);
        }

        Student student;
        try
        {
            student = new Student(
                user.Id,
                command.StudentNumber,
                command.NameEnglish,
                command.NameArabic,
                command.DepartmentId,
                command.EnrollmentYear,
                command.AcademicLevel);
        }
        catch (ArgumentException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.Students.Add(student);
        var saveResult = await SaveCreatedAsync(student.Id, cancellationToken);
        if (!saveResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return saveResult;
        }

        await transaction.CommitAsync(cancellationToken);
        return saveResult;
    }

    public async Task<OperationResult> UpdateProfileAsync(Guid id, StudentProfileCommand command, CancellationToken cancellationToken = default)
    {
        var student = await DbContext.Students.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (student is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var departmentIsActive = await DbContext.Departments.AnyAsync(department => department.Id == command.DepartmentId && department.IsActive, cancellationToken);
        if (!departmentIsActive)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveDepartmentRequired"));
        }

        var studentNumber = NormalizeCode(command.StudentNumber);
        if (await DbContext.Students.AnyAsync(item => item.Id != id && item.StudentNumber == studentNumber, cancellationToken))
        {
            return DuplicateFailure(nameof(command.StudentNumber), "ErrorStudentNumberExists", studentNumber);
        }

        try
        {
            SetOriginalRowVersion(student, command.RowVersion);
            student.UpdateProfile(command.StudentNumber, command.NameEnglish, command.NameArabic, command.DepartmentId, command.EnrollmentYear, command.AcademicLevel);
        }
        catch (ArgumentException exception)
        {
            return OperationResult.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
        if (user is not null)
        {
            user.FullName = command.NameEnglish.Trim();
            user.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return await SaveChangesAsync(cancellationToken);
    }

    public Task<OperationResult> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(id, true, cancellationToken);

    public Task<OperationResult> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => SetActiveAsync(id, false, cancellationToken);

    public async Task<OperationResult> ResetTemporaryPasswordAsync(Guid id, TemporaryPasswordCommand command, CancellationToken cancellationToken = default)
    {
        if (command.TemporaryPassword != command.ConfirmTemporaryPassword)
        {
            return ValidationFailure(nameof(command.ConfirmTemporaryPassword), "ValidationPasswordConfirm");
        }

        var student = await DbContext.Students.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (student is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
        if (user is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(user, token, command.TemporaryPassword);
        if (!resetResult.Succeeded)
        {
            return IdentityFailure(resetResult);
        }

        user.MustChangePassword = true;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _userManager.UpdateSecurityStampAsync(user);
        var updateResult = await _userManager.UpdateAsync(user);
        return updateResult.Succeeded ? OperationResult.Success() : IdentityFailure(updateResult);
    }

    private async Task<OperationResult> ValidateCreateAsync(StudentAccountCommand command, CancellationToken cancellationToken)
    {
        var email = NormalizeText(command.Email);
        if (!EmailValidator.IsValid(email))
        {
            return ValidationFailure(nameof(command.Email), "ValidationEmail");
        }

        if (command.TemporaryPassword != command.ConfirmTemporaryPassword)
        {
            return ValidationFailure(nameof(command.ConfirmTemporaryPassword), "ValidationPasswordConfirm");
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return DuplicateFailure(nameof(command.Email), "ErrorEmailExists", email);
        }

        var studentNumber = NormalizeCode(command.StudentNumber);
        if (await DbContext.Students.AnyAsync(student => student.StudentNumber == studentNumber, cancellationToken))
        {
            return DuplicateFailure(nameof(command.StudentNumber), "ErrorStudentNumberExists", studentNumber);
        }

        var departmentIsActive = await DbContext.Departments.AnyAsync(department => department.Id == command.DepartmentId && department.IsActive, cancellationToken);
        return departmentIsActive ? OperationResult.Success() : OperationResult.Failure(OperationErrors.Dependency("ErrorActiveDepartmentRequired"));
    }

    private async Task<OperationResult> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var student = await DbContext.Students.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (student is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var user = await _userManager.FindByIdAsync(student.ApplicationUserId);
        if (user is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (isActive)
        {
            student.Activate();
            user.IsActive = true;
            user.IsDisabled = false;
        }
        else
        {
            student.Deactivate();
            user.IsActive = false;
            user.IsDisabled = true;
        }

        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _userManager.UpdateSecurityStampAsync(user);
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return IdentityFailure(updateResult);
        }

        return await SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }
    }

    private static StudentDto ToDto(Student student, ApplicationUser user)
        => new(
            student.Id,
            student.ApplicationUserId,
            user.Email ?? string.Empty,
            student.StudentNumber,
            student.NameEnglish,
            student.NameArabic,
            student.DepartmentId,
            student.Department?.NameEnglish ?? string.Empty,
            student.Department?.NameArabic ?? string.Empty,
            student.EnrollmentYear,
            student.AcademicLevel,
            student.IsActive && user.IsActive && !user.IsDisabled,
            user.MustChangePassword,
            student.CreatedAtUtc,
            student.UpdatedAtUtc,
            RowVersion(student.RowVersion));

    private static OperationResult IdentityFailure(IdentityResult identityResult)
        => OperationResult.Failure(identityResult.Errors.Select(error => OperationErrors.Identity("ErrorIdentityMessage", error.Description)).ToArray());

    private static OperationResult<T> IdentityFailure<T>(IdentityResult identityResult)
        => OperationResult<T>.Failure(identityResult.Errors.Select(error => OperationErrors.Identity("ErrorIdentityMessage", error.Description)).ToArray());
}
