using AttendAI.Application.Academic;
using AttendAI.Application.Biometrics;
using AttendAI.Domain.Academic;
using AttendAI.Application.FaceVerification;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.IntegrationTests;

public sealed class FaceVerificationServiceTests : IClassFixture<IdentityCookieWebApplicationFactory>
{
    private const string Password = "Temp!12345A";
    private readonly IdentityCookieWebApplicationFactory _factory;

    public FaceVerificationServiceTests(IdentityCookieWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task VerifyCurrentStudentAsync_records_match_and_no_match_attempts_without_attendance_side_effects()
    {
        using var scope = _factory.Services.CreateScope();
        var userId = await CreateEnrolledStudentAsync(scope, "VERIFY_MATCH");
        var verifier = scope.ServiceProvider.GetRequiredService<IFaceVerificationService>();

        var match = await verifier.VerifyCurrentStudentAsync(
            userId,
            new FaceVerificationCommand(Sample("VERIFY_MATCH"), "match-request"));
        var noMatch = await verifier.VerifyCurrentStudentAsync(
            userId,
            new FaceVerificationCommand(Sample("VERIFY_OTHER"), "no-match-request"));

        Assert.True(match.Succeeded, FormatErrors(match.Errors));
        Assert.True(noMatch.Succeeded, FormatErrors(noMatch.Errors));
        Assert.Equal(FaceVerificationOutcome.Matched, match.Value!.Outcome);
        Assert.Equal(FaceVerificationDecision.Match, match.Value.Decision);
        Assert.Equal(FaceVerificationOutcome.NotMatched, noMatch.Value!.Outcome);
        Assert.Equal(FaceVerificationDecision.NoMatch, noMatch.Value.Decision);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(2, await dbContext.FaceVerificationAttempts.CountAsync(attempt => attempt.Student!.ApplicationUserId == userId));
        Assert.DoesNotContain(dbContext.Model.GetEntityTypes(), entity => entity.GetTableName() == "AttendanceRecords");
    }

    [Fact]
    public async Task VerifyCurrentStudentAsync_is_idempotent_for_client_request_id()
    {
        using var scope = _factory.Services.CreateScope();
        var userId = await CreateEnrolledStudentAsync(scope, "VERIFY_IDEMPOTENT");
        var verifier = scope.ServiceProvider.GetRequiredService<IFaceVerificationService>();

        var first = await verifier.VerifyCurrentStudentAsync(
            userId,
            new FaceVerificationCommand(Sample("VERIFY_IDEMPOTENT"), "same-request"));
        var second = await verifier.VerifyCurrentStudentAsync(
            userId,
            new FaceVerificationCommand(Sample("VERIFY_IDEMPOTENT"), "same-request"));

        Assert.True(first.Succeeded, FormatErrors(first.Errors));
        Assert.True(second.Succeeded, FormatErrors(second.Errors));
        Assert.Equal(first.Value!.AttemptId, second.Value!.AttemptId);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await dbContext.FaceVerificationAttempts.CountAsync(attempt => attempt.Student!.ApplicationUserId == userId));
    }

    [Fact]
    public async Task VerifyCurrentStudentAsync_marks_incompatible_active_template_for_re_enrollment()
    {
        using var scope = _factory.Services.CreateScope();
        var userId = await CreateEnrolledStudentAsync(scope, "VERIFY_INCOMPATIBLE");
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var template = await dbContext.StudentFaceTemplates
            .Include(item => item.Student)
            .SingleAsync(item => item.Student!.ApplicationUserId == userId && item.IsActive);
        dbContext.Entry(template).Property(nameof(StudentFaceTemplate.ModelVersion)).CurrentValue = "incompatible-test-version";
        await dbContext.SaveChangesAsync();

        var verifier = scope.ServiceProvider.GetRequiredService<IFaceVerificationService>();

        var result = await verifier.VerifyCurrentStudentAsync(
            userId,
            new FaceVerificationCommand(Sample("VERIFY_INCOMPATIBLE"), "incompatible-request"));

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        Assert.Equal(FaceVerificationOutcome.IncompatibleTemplate, result.Value!.Outcome);

        var revokedTemplate = await dbContext.StudentFaceTemplates
            .IgnoreQueryFilters()
            .SingleAsync(item => item.Id == template.Id);
        Assert.False(revokedTemplate.IsActive);
        Assert.True(revokedTemplate.RequiresReEnrollment);
        Assert.NotNull(revokedTemplate.RevokedAtUtc);
        Assert.Contains("incompatible", revokedTemplate.RevocationReason, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string> CreateEnrolledStudentAsync(IServiceScope scope, string marker)
    {
        var departmentId = await CreateDepartmentAsync(scope);
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var studentNumber = $"FV{Guid.NewGuid():N}"[..12].ToUpperInvariant();
        var createResult = await students.CreateAccountAsync(new StudentAccountCommand
        {
            Email = $"{studentNumber.ToLowerInvariant()}@attendai.test",
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            StudentNumber = studentNumber,
            NameEnglish = "Face Verification Student",
            NameArabic = "طالب تحقق",
            DepartmentId = departmentId,
            EnrollmentYear = 2026,
            AcademicLevel = "First"
        });
        Assert.True(createResult.Succeeded, FormatErrors(createResult.Errors));

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dto = await students.GetByIdAsync(createResult.Value);
        var user = await userManager.FindByIdAsync(dto!.ApplicationUserId);
        Assert.NotNull(user);
        user.MustChangePassword = false;
        await userManager.UpdateAsync(user);

        var biometrics = scope.ServiceProvider.GetRequiredService<IBiometricEnrollmentService>();
        var consent = await biometrics.AcceptConsentAsync(user.Id);
        Assert.True(consent.Succeeded, FormatErrors(consent.Errors));

        var enrollment = await biometrics.EnrollAsync(
            user.Id,
            new FaceEnrollmentCommand([Sample(marker), Sample(marker), Sample(marker)]));
        Assert.True(enrollment.Succeeded, FormatErrors(enrollment.Errors));

        return user.Id;
    }

    private static async Task<Guid> CreateDepartmentAsync(IServiceScope scope)
    {
        var departments = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
        var code = $"D{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var result = await departments.CreateAsync(new DepartmentCommand
        {
            Code = code,
            NameEnglish = $"Department {code}",
            NameArabic = $"قسم {code}"
        });

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static FaceCaptureSample Sample(string marker)
        => new(Png(160, 120, marker), "image/png", "capture");

    private static byte[] Png(int width, int height, string marker)
    {
        var bytes = new List<byte>
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52
        };
        bytes.AddRange(Int32BigEndian(width));
        bytes.AddRange(Int32BigEndian(height));
        bytes.AddRange([0x08, 0x02, 0x00, 0x00, 0x00]);
        bytes.AddRange(System.Text.Encoding.UTF8.GetBytes(marker));
        return bytes.ToArray();
    }

    private static byte[] Int32BigEndian(int value)
        => [(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value];

    private static string FormatErrors(IEnumerable<AttendAI.Application.Common.Models.OperationError> errors)
        => string.Join("; ", errors.Select(error => $"{error.Code}:{error.MessageKey}"));
}
