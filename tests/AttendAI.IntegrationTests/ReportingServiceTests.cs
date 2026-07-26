using AttendAI.Application.Reporting;
using AttendAI.Domain.Academic;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.IntegrationTests;

public sealed class ReportingServiceTests : IClassFixture<AttendAiWebApplicationFactory>
{
    private readonly AttendAiWebApplicationFactory _factory;

    public ReportingServiceTests(AttendAiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Reports_derive_missed_sessions_without_counting_future_cancelled_or_disabled_sessions()
    {
        using var scope = _factory.Services.CreateScope();
        var scenario = await SeedReportingScenarioAsync(scope, "report-student-" + Guid.NewGuid().ToString("N"), "report-instructor-" + Guid.NewGuid().ToString("N"));
        var reporting = scope.ServiceProvider.GetRequiredService<IAttendanceReportingService>();

        var filter = new AttendanceReportFilter
        {
            FromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            ToDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PageSize = 10
        };

        var studentReport = await reporting.GetStudentReportAsync(scenario.StudentUserId, filter);
        var instructorReport = await reporting.GetInstructorReportAsync(scenario.InstructorUserId, filter);
        var adminReport = await reporting.GetAdminReportAsync(new AttendanceReportFilter
        {
            StudentId = scenario.StudentId,
            FromDate = filter.FromDate,
            ToDate = filter.ToDate,
            PageSize = 10
        });

        Assert.NotNull(studentReport);
        Assert.NotNull(instructorReport);
        Assert.Equal(2, studentReport!.Summary.EligibleSessionCount);
        Assert.Equal(1, studentReport.Summary.PresentCount);
        Assert.Equal(0, studentReport.Summary.LateCount);
        Assert.Equal(1, studentReport.Summary.MissedCount);
        Assert.Equal(50, studentReport.Summary.AttendancePercentage);
        Assert.Equal(studentReport.Summary, instructorReport!.Summary);
        Assert.Equal(studentReport.Summary, adminReport.Summary);
        Assert.Contains(studentReport.Rows.Items, row => row.ReportStatus == AttendanceReportStatus.Present);
        Assert.Contains(studentReport.Rows.Items, row => row.ReportStatus == AttendanceReportStatus.Missed);
    }

    [Fact]
    public async Task Admin_audit_report_uses_safe_attendance_event_fields()
    {
        using var scope = _factory.Services.CreateScope();
        await SeedReportingScenarioAsync(scope, "audit-student-" + Guid.NewGuid().ToString("N"), "audit-instructor-" + Guid.NewGuid().ToString("N"));
        var reporting = scope.ServiceProvider.GetRequiredService<IAttendanceReportingService>();

        var report = await reporting.GetAdminAuditReportAsync(new AttendanceAuditFilter
        {
            Category = AuditEventCategory.Attendance,
            PageSize = 10
        });

        var attendanceEvent = report.Events.Items.First(item => item.Category == AuditEventCategory.Attendance);
        Assert.Contains("Succeeded", attendanceEvent.Outcome, StringComparison.Ordinal);
        Assert.DoesNotContain("0.95", attendanceEvent.SafeDescription, StringComparison.Ordinal);
        Assert.DoesNotContain("challenge", attendanceEvent.SafeDescription, StringComparison.OrdinalIgnoreCase);
    }

    internal static async Task<ReportingScenario> SeedReportingScenarioAsync(IServiceScope scope, string studentUserId, string instructorUserId)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var studentUser = TestUser(studentUserId, "student-" + suffix);
        var instructorUser = studentUserId == instructorUserId
            ? null
            : TestUser(instructorUserId, "instructor-" + suffix);
        var department = new Department("RD" + suffix, "Reports Department", "قسم التقارير");
        var course = new Course("RC" + suffix[..4], "Reporting Course", "مساق التقارير", department.Id, 3);
        var section = new Section(course.Id, "R" + suffix[..3], "2026/2027", Semester.First, 30);
        var classroom = new Classroom("RR" + suffix[..4], "Reports Building", "مبنى التقارير", "101", 40, 31.9539m, 35.9106m);
        var student = new Student(studentUserId, "RS" + suffix, "Reporting Student", "طالب التقارير", department.Id, 2026, "First");
        var instructor = new Instructor(instructorUserId, "RI" + suffix, "Reporting Instructor", "مدرس التقارير", department.Id, "Lecturer");
        var enrollment = new StudentEnrollment(student.Id, section.Id);
        var schedule = new LectureSchedule(
            section.Id,
            instructor.Id,
            classroom.Id,
            now.DayOfWeek,
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            DateOnly.FromDateTime(now.AddDays(-30).Date),
            DateOnly.FromDateTime(now.AddDays(30).Date),
            10,
            50);

        var presentSession = CompletedSession(schedule.Id, section.Id, instructor.Id, classroom.Id, now.AddDays(-4), "present");
        var missedSession = CompletedSession(schedule.Id, section.Id, instructor.Id, classroom.Id, now.AddDays(-3), "missed");
        var futureSession = new LectureSession(
            schedule.Id,
            section.Id,
            instructor.Id,
            classroom.Id,
            DateOnly.FromDateTime(now.AddDays(2).Date),
            now.AddDays(2),
            now.AddDays(2).AddHours(1),
            now.AddDays(2),
            "future-hash",
            "future-protected",
            now.AddDays(2).AddMinutes(30),
            10,
            50,
            instructorUserId,
            31.9539m,
            35.9106m);
        var cancelledSession = StartedSession(schedule.Id, section.Id, instructor.Id, classroom.Id, now.AddDays(-2), "cancelled");
        cancelledSession.Cancel(instructorUserId, now.AddDays(-2).AddHours(1), "Cancelled for test");
        var disabledSession = new LectureSession(
            schedule.Id,
            section.Id,
            instructor.Id,
            classroom.Id,
            DateOnly.FromDateTime(now.AddDays(-1).Date),
            now.AddDays(-1),
            now.AddDays(-1).AddHours(1),
            now.AddDays(-1),
            "disabled-hash",
            "disabled-protected",
            now.AddDays(-1).AddMinutes(30),
            10,
            50,
            instructorUserId,
            31.9539m,
            35.9106m,
            attendanceCheckInEnabled: false);
        disabledSession.End(instructorUserId, now.AddDays(-1).AddHours(1), "Disabled for test");

        var faceAttempt = new FaceVerificationAttempt(
            student.Id,
            null,
            FaceVerificationPurpose.FutureAttendance,
            FaceVerificationOutcome.Matched,
            FaceVerificationDecision.Match,
            null,
            0.95m,
            0.363m,
            ScoreMetric.CosineSimilarity,
            "OpenCV-SFace",
            "test",
            "SFace",
            "test",
            "opencv-sface-f32le-v1",
            now.AddDays(-4).AddMinutes(5),
            "client",
            "FaceVerificationAttemptDescriptionMatched");
        var attempt = new AttendanceAttempt(
            student.Id,
            presentSession.Id,
            AttendanceAttemptOutcome.Succeeded,
            AttendanceFailureReason.None,
            LocationVerificationOutcome.Accepted,
            now.AddDays(-4).AddMinutes(5),
            "idempotency-" + suffix,
            "challenge-" + suffix,
            "AttendanceAttemptDescriptionSucceeded",
            0m,
            10m,
            50,
            75,
            1);
        attempt.AttachFaceVerificationAttempt(faceAttempt.Id);
        var record = new AttendanceRecord(
            presentSession.Id,
            student.Id,
            attempt.Id,
            faceAttempt.Id,
            AttendanceStatus.Present,
            now.AddDays(-4).AddMinutes(5),
            classroom.Id,
            50,
            75,
            0m,
            10m);

        dbContext.AddRange(
            studentUser,
            department,
            course,
            section,
            classroom,
            student,
            instructor,
            enrollment,
            schedule,
            presentSession,
            missedSession,
            futureSession,
            cancelledSession,
            disabledSession,
            faceAttempt,
            attempt,
            record);
        if (instructorUser is not null)
        {
            dbContext.Users.Add(instructorUser);
        }

        await dbContext.SaveChangesAsync();

        return new ReportingScenario(student.Id, studentUserId, instructor.Id, instructorUserId);
    }

    private static LectureSession CompletedSession(
        Guid scheduleId,
        Guid sectionId,
        Guid instructorId,
        Guid classroomId,
        DateTimeOffset start,
        string suffix)
    {
        var session = StartedSession(scheduleId, sectionId, instructorId, classroomId, start, suffix);
        session.End("reporting-test-user", start.AddHours(1), "Completed for reporting test");
        return session;
    }

    private static LectureSession StartedSession(
        Guid scheduleId,
        Guid sectionId,
        Guid instructorId,
        Guid classroomId,
        DateTimeOffset start,
        string suffix)
        => new(
            scheduleId,
            sectionId,
            instructorId,
            classroomId,
            DateOnly.FromDateTime(start.Date),
            start,
            start.AddHours(1),
            start,
            suffix + "-hash",
            suffix + "-protected",
            start.AddMinutes(30),
            10,
            50,
            "reporting-test-user",
            31.9539m,
            35.9106m);

    private static ApplicationUser TestUser(string userId, string emailPrefix)
    {
        var email = emailPrefix + "@attendai.test";
        return new ApplicationUser
        {
            Id = userId,
            UserName = email,
            NormalizedUserName = email.ToUpperInvariant(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            EmailConfirmed = true,
            FullName = emailPrefix,
            IsActive = true,
            IsDisabled = false,
            MustChangePassword = false,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ConcurrencyStamp = Guid.NewGuid().ToString("N")
        };
    }

    internal sealed record ReportingScenario(
        Guid StudentId,
        string StudentUserId,
        Guid InstructorId,
        string InstructorUserId);
}
