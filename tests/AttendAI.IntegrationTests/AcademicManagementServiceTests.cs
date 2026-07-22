using System.Net;
using System.Text.RegularExpressions;
using AttendAI.Application.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Identity;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.IntegrationTests;

public sealed class AcademicManagementServiceTests : IClassFixture<IdentityCookieWebApplicationFactory>
{
    private const string Password = "Temp!12345A";
    private static readonly Regex AntiforgeryTokenRegex = new(
        "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    private readonly IdentityCookieWebApplicationFactory _factory;

    public AcademicManagementServiceTests(IdentityCookieWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Student_account_creation_assigns_role_and_requires_password_change()
    {
        using var scope = _factory.Services.CreateScope();
        var departmentId = await CreateDepartmentAsync(scope);
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();

        var result = await students.CreateAccountAsync(new StudentAccountCommand
        {
            Email = UniqueEmail("student"),
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            StudentNumber = UniqueCode("S"),
            NameEnglish = "Integration Student",
            NameArabic = "طالب تكامل",
            DepartmentId = departmentId,
            EnrollmentYear = 2026,
            AcademicLevel = "First"
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));

        var dto = await students.GetByIdAsync(result.Value);
        Assert.NotNull(dto);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(dto.Email);
        Assert.NotNull(user);
        Assert.True(user.MustChangePassword);
        Assert.True(await userManager.IsInRoleAsync(user, "Student"));
    }

    [Fact]
    public async Task Temporary_password_login_redirects_to_change_password()
    {
        using var scope = _factory.Services.CreateScope();
        var departmentId = await CreateDepartmentAsync(scope);
        var email = UniqueEmail("student-login");
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var createResult = await students.CreateAccountAsync(new StudentAccountCommand
        {
            Email = email,
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            StudentNumber = UniqueCode("SL"),
            NameEnglish = "Temporary Student",
            NameArabic = "طالب مؤقت",
            DepartmentId = departmentId,
            EnrollmentYear = 2026,
            AcademicLevel = "First"
        });
        Assert.True(createResult.Succeeded, FormatErrors(createResult.Errors));

        var client = CreateCookieClient();
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Login");
        var response = await PostFormAsync(client, "/Account/Login", new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = email,
            ["Password"] = Password,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = "/"
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/ChangePassword", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Deactivated_student_account_cannot_login()
    {
        using var scope = _factory.Services.CreateScope();
        var departmentId = await CreateDepartmentAsync(scope);
        var email = UniqueEmail("student-disabled");
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var createResult = await students.CreateAccountAsync(new StudentAccountCommand
        {
            Email = email,
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            StudentNumber = UniqueCode("SD"),
            NameEnglish = "Disabled Student",
            NameArabic = "طالب معطل",
            DepartmentId = departmentId,
            EnrollmentYear = 2026,
            AcademicLevel = "First"
        });
        Assert.True(createResult.Succeeded, FormatErrors(createResult.Errors));
        Assert.True((await students.DeactivateAsync(createResult.Value)).Succeeded);

        var client = CreateCookieClient();
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Login");
        var response = await PostFormAsync(client, "/Account/Login", new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = email,
            ["Password"] = Password,
            ["RememberMe"] = "false",
            ["ReturnUrl"] = "/"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain(response.Headers, header => header.Key == "Set-Cookie" && header.Value.Any(value => value.Contains("__Host-AttendAI.Auth", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task Duplicate_enrollment_and_capacity_are_rejected()
    {
        using var scope = _factory.Services.CreateScope();
        var departmentId = await CreateDepartmentAsync(scope);
        var courseId = await CreateCourseAsync(scope, departmentId);
        var sectionId = await CreateSectionAsync(scope, courseId, capacity: 1);
        var firstStudentId = await CreateStudentAsync(scope, departmentId, "E1");
        var secondStudentId = await CreateStudentAsync(scope, departmentId, "E2");
        var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        var first = await enrollments.CreateAsync(new EnrollmentCommand { StudentId = firstStudentId, SectionId = sectionId });
        var duplicate = await enrollments.CreateAsync(new EnrollmentCommand { StudentId = firstStudentId, SectionId = sectionId });
        var overCapacity = await enrollments.CreateAsync(new EnrollmentCommand { StudentId = secondStudentId, SectionId = sectionId });

        Assert.True(first.Succeeded, FormatErrors(first.Errors));
        Assert.False(duplicate.Succeeded);
        Assert.Contains(duplicate.Errors, error => error.MessageKey == "ErrorDuplicateEnrollment");
        Assert.False(overCapacity.Succeeded);
        Assert.Contains(overCapacity.Errors, error => error.MessageKey == "ErrorSectionCapacityReached");
    }

    [Fact]
    public async Task Section_allows_only_one_active_primary_instructor()
    {
        using var scope = _factory.Services.CreateScope();
        var departmentId = await CreateDepartmentAsync(scope);
        var courseId = await CreateCourseAsync(scope, departmentId);
        var sectionId = await CreateSectionAsync(scope, courseId, capacity: 30);
        var firstInstructorId = await CreateInstructorAsync(scope, departmentId, "I1");
        var secondInstructorId = await CreateInstructorAsync(scope, departmentId, "I2");
        var assignments = scope.ServiceProvider.GetRequiredService<IInstructorAssignmentService>();

        var first = await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = firstInstructorId, SectionId = sectionId, IsPrimary = true });
        var secondPrimary = await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = secondInstructorId, SectionId = sectionId, IsPrimary = true });

        Assert.True(first.Succeeded, FormatErrors(first.Errors));
        Assert.False(secondPrimary.Succeeded);
        Assert.Contains(secondPrimary.Errors, error => error.MessageKey == "ErrorPrimaryInstructorExists");
    }

    private HttpClient CreateCookieClient()
        => _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();
        var tokenMatch = AntiforgeryTokenRegex.Match(html);
        Assert.True(tokenMatch.Success);
        return tokenMatch.Groups[1].Value;
    }

    private static Task<HttpResponseMessage> PostFormAsync(HttpClient client, string path, Dictionary<string, string> formFields)
        => client.PostAsync(path, new FormUrlEncodedContent(formFields));

    private static async Task<Guid> CreateDepartmentAsync(IServiceScope scope)
    {
        var departments = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
        var code = UniqueCode("D");
        var result = await departments.CreateAsync(new DepartmentCommand
        {
            Code = code,
            NameEnglish = $"Department {code}",
            NameArabic = $"قسم {code}"
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateCourseAsync(IServiceScope scope, Guid departmentId)
    {
        var courses = scope.ServiceProvider.GetRequiredService<ICourseService>();
        var code = UniqueCode("C");
        var result = await courses.CreateAsync(new CourseCommand
        {
            Code = code,
            NameEnglish = $"Course {code}",
            NameArabic = $"مساق {code}",
            DepartmentId = departmentId,
            CreditHours = 3
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateSectionAsync(IServiceScope scope, Guid courseId, int capacity)
    {
        var sections = scope.ServiceProvider.GetRequiredService<ISectionService>();
        var result = await sections.CreateAsync(new SectionCommand
        {
            CourseId = courseId,
            SectionNumber = UniqueCode("SEC"),
            AcademicYear = "2026/2027",
            Semester = Semester.First,
            Capacity = capacity
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateStudentAsync(IServiceScope scope, Guid departmentId, string prefix)
    {
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var result = await students.CreateAccountAsync(new StudentAccountCommand
        {
            Email = UniqueEmail(prefix),
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            StudentNumber = UniqueCode(prefix),
            NameEnglish = $"Student {prefix}",
            NameArabic = $"طالب {prefix}",
            DepartmentId = departmentId,
            EnrollmentYear = 2026,
            AcademicLevel = "First"
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateInstructorAsync(IServiceScope scope, Guid departmentId, string prefix)
    {
        var instructors = scope.ServiceProvider.GetRequiredService<IInstructorService>();
        var result = await instructors.CreateAccountAsync(new InstructorAccountCommand
        {
            Email = UniqueEmail(prefix),
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            EmployeeNumber = UniqueCode(prefix),
            NameEnglish = $"Instructor {prefix}",
            NameArabic = $"مدرس {prefix}",
            DepartmentId = departmentId,
            AcademicTitle = "Lecturer"
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static string UniqueCode(string prefix)
        => $"{prefix}{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 12, 32)];

    private static string UniqueEmail(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}@attendai.local";

    private static string FormatErrors(IReadOnlyList<Application.Common.Models.OperationError> errors)
        => string.Join("; ", errors.Select(error => error.MessageKey));
}
