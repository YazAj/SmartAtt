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

public sealed class InstructorService : AcademicServiceBase, IInstructorService
{
    private static readonly EmailAddressAttribute EmailValidator = new();
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstructorService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
        : base(dbContext)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PagedResult<InstructorDto>> GetPagedAsync(InstructorQuery query, CancellationToken cancellationToken = default)
    {
        var instructors =
            from instructor in DbContext.Instructors.Include(instructor => instructor.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on instructor.ApplicationUserId equals user.Id
            select new { Instructor = instructor, User = user };

        var search = NormalizeText(query.Search);
        if (search.Length > 0)
        {
            instructors = instructors.Where(item =>
                item.Instructor.EmployeeNumber.Contains(search) ||
                item.Instructor.NameEnglish.Contains(search) ||
                item.Instructor.NameArabic.Contains(search) ||
                item.User.Email!.Contains(search));
        }

        if (query.DepartmentId.HasValue)
        {
            instructors = instructors.Where(item => item.Instructor.DepartmentId == query.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.AcademicTitle))
        {
            instructors = instructors.Where(item => item.Instructor.AcademicTitle.Contains(query.AcademicTitle));
        }

        if (query.IsActive.HasValue)
        {
            instructors = instructors.Where(item => item.Instructor.IsActive == query.IsActive.Value);
        }

        instructors = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending ? instructors.OrderByDescending(item => item.Instructor.NameEnglish) : instructors.OrderBy(item => item.Instructor.NameEnglish),
            "email" => query.SortDescending ? instructors.OrderByDescending(item => item.User.Email) : instructors.OrderBy(item => item.User.Email),
            "department" => query.SortDescending ? instructors.OrderByDescending(item => item.Instructor.Department!.NameEnglish) : instructors.OrderBy(item => item.Instructor.Department!.NameEnglish),
            "title" => query.SortDescending ? instructors.OrderByDescending(item => item.Instructor.AcademicTitle) : instructors.OrderBy(item => item.Instructor.AcademicTitle),
            "status" => query.SortDescending ? instructors.OrderByDescending(item => item.Instructor.IsActive) : instructors.OrderBy(item => item.Instructor.IsActive),
            _ => query.SortDescending ? instructors.OrderByDescending(item => item.Instructor.EmployeeNumber) : instructors.OrderBy(item => item.Instructor.EmployeeNumber)
        };

        var total = await instructors.CountAsync(cancellationToken);
        var rows = await ApplyPaging(instructors, query).ToListAsync(cancellationToken);
        var items = rows.Select(item => ToDto(item.Instructor, item.User)).ToList();
        return new PagedResult<InstructorDto>(items, query.PageNumber, query.PageSize, total);
    }

    public async Task<InstructorDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await (
            from instructor in DbContext.Instructors.Include(instructor => instructor.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on instructor.ApplicationUserId equals user.Id
            where instructor.Id == id
            select new { Instructor = instructor, User = user }).FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToDto(row.Instructor, row.User);
    }

    public async Task<InstructorDto?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var row = await (
            from instructor in DbContext.Instructors.Include(instructor => instructor.Department).AsNoTracking()
            join user in DbContext.Users.AsNoTracking() on instructor.ApplicationUserId equals user.Id
            where instructor.ApplicationUserId == userId
            select new { Instructor = instructor, User = user }).FirstOrDefaultAsync(cancellationToken);

        return row is null ? null : ToDto(row.Instructor, row.User);
    }

    public async Task<OperationResult<Guid>> CreateAccountAsync(InstructorAccountCommand command, CancellationToken cancellationToken = default)
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

        await EnsureRoleAsync(RoleConstants.Instructor);
        var roleResult = await _userManager.AddToRoleAsync(user, RoleConstants.Instructor);
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return IdentityFailure<Guid>(roleResult);
        }

        Instructor instructor;
        try
        {
            instructor = new Instructor(
                user.Id,
                command.EmployeeNumber,
                command.NameEnglish,
                command.NameArabic,
                command.DepartmentId,
                command.AcademicTitle);
        }
        catch (ArgumentException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            return OperationResult<Guid>.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        DbContext.Instructors.Add(instructor);
        var saveResult = await SaveCreatedAsync(instructor.Id, cancellationToken);
        if (!saveResult.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            return saveResult;
        }

        await transaction.CommitAsync(cancellationToken);
        return saveResult;
    }

    public async Task<OperationResult> UpdateProfileAsync(Guid id, InstructorProfileCommand command, CancellationToken cancellationToken = default)
    {
        var instructor = await DbContext.Instructors.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (instructor is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var departmentIsActive = await DbContext.Departments.AnyAsync(department => department.Id == command.DepartmentId && department.IsActive, cancellationToken);
        if (!departmentIsActive)
        {
            return OperationResult.Failure(OperationErrors.Dependency("ErrorActiveDepartmentRequired"));
        }

        var employeeNumber = NormalizeCode(command.EmployeeNumber);
        if (await DbContext.Instructors.AnyAsync(item => item.Id != id && item.EmployeeNumber == employeeNumber, cancellationToken))
        {
            return DuplicateFailure(nameof(command.EmployeeNumber), "ErrorEmployeeNumberExists", employeeNumber);
        }

        try
        {
            SetOriginalRowVersion(instructor, command.RowVersion);
            instructor.UpdateProfile(command.EmployeeNumber, command.NameEnglish, command.NameArabic, command.DepartmentId, command.AcademicTitle);
        }
        catch (ArgumentException exception)
        {
            return OperationResult.Failure(OperationErrors.Validation(exception.ParamName ?? string.Empty, "ErrorValidationMessage", exception.Message));
        }

        var user = await _userManager.FindByIdAsync(instructor.ApplicationUserId);
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

        var instructor = await DbContext.Instructors.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (instructor is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var user = await _userManager.FindByIdAsync(instructor.ApplicationUserId);
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

    private async Task<OperationResult> ValidateCreateAsync(InstructorAccountCommand command, CancellationToken cancellationToken)
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

        var employeeNumber = NormalizeCode(command.EmployeeNumber);
        if (await DbContext.Instructors.AnyAsync(instructor => instructor.EmployeeNumber == employeeNumber, cancellationToken))
        {
            return DuplicateFailure(nameof(command.EmployeeNumber), "ErrorEmployeeNumberExists", employeeNumber);
        }

        var departmentIsActive = await DbContext.Departments.AnyAsync(department => department.Id == command.DepartmentId && department.IsActive, cancellationToken);
        return departmentIsActive ? OperationResult.Success() : OperationResult.Failure(OperationErrors.Dependency("ErrorActiveDepartmentRequired"));
    }

    private async Task<OperationResult> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var instructor = await DbContext.Instructors.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (instructor is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        var user = await _userManager.FindByIdAsync(instructor.ApplicationUserId);
        if (user is null)
        {
            return OperationResult.Failure(OperationErrors.NotFound());
        }

        if (isActive)
        {
            instructor.Activate();
            user.IsActive = true;
            user.IsDisabled = false;
        }
        else
        {
            instructor.Deactivate();
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

    private static InstructorDto ToDto(Instructor instructor, ApplicationUser user)
        => new(
            instructor.Id,
            instructor.ApplicationUserId,
            user.Email ?? string.Empty,
            instructor.EmployeeNumber,
            instructor.NameEnglish,
            instructor.NameArabic,
            instructor.DepartmentId,
            instructor.Department?.NameEnglish ?? string.Empty,
            instructor.Department?.NameArabic ?? string.Empty,
            instructor.AcademicTitle,
            instructor.IsActive && user.IsActive && !user.IsDisabled,
            user.MustChangePassword,
            instructor.CreatedAtUtc,
            instructor.UpdatedAtUtc,
            RowVersion(instructor.RowVersion));

    private static OperationResult IdentityFailure(IdentityResult identityResult)
        => OperationResult.Failure(identityResult.Errors.Select(error => OperationErrors.Identity("ErrorIdentityMessage", error.Description)).ToArray());

    private static OperationResult<T> IdentityFailure<T>(IdentityResult identityResult)
        => OperationResult<T>.Failure(identityResult.Errors.Select(error => OperationErrors.Identity("ErrorIdentityMessage", error.Description)).ToArray());
}
