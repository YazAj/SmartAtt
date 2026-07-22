using System.Net;
using System.Globalization;
using AttendAI.Application.Academic;
using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.Lectures;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AttendAI.IntegrationTests;

public sealed class LectureSchedulingServiceTests : IClassFixture<LectureSchedulingWebApplicationFactory>
{
    private const string Password = "Temp!12345A";
    private readonly LectureSchedulingWebApplicationFactory _factory;

    public LectureSchedulingServiceTests(LectureSchedulingWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Admin_schedule_create_and_conflict_detection_cover_protected_resources()
    {
        using var scope = _factory.Services.CreateScope();
        var baseData = await CreateAcademicScenarioAsync(scope);
        var secondSection = await CreateSectionAsync(scope, baseData.CourseId, 40);
        var secondInstructor = await CreateInstructorAsync(scope, baseData.DepartmentId, "CONI2");
        var secondInstructorUserId = (await scope.ServiceProvider.GetRequiredService<IInstructorService>().GetByIdAsync(secondInstructor))!.ApplicationUserId;
        var secondClassroom = await CreateClassroomAsync(scope, "CONC2", 100);
        var assignments = scope.ServiceProvider.GetRequiredService<IInstructorAssignmentService>();
        Assert.True((await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = secondInstructor, SectionId = secondSection })).Succeeded);
        Assert.True((await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = baseData.InstructorId, SectionId = secondSection })).Succeeded);
        Assert.True((await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = secondInstructor, SectionId = baseData.SectionId })).Succeeded);

        var schedules = scope.ServiceProvider.GetRequiredService<ILectureScheduleService>();
        var first = await schedules.CreateAsync(ScheduleCommand(baseData.SectionId, baseData.InstructorId, baseData.ClassroomId));
        Assert.True(first.Succeeded, FormatErrors(first.Errors));

        var classroomConflict = await schedules.CreateAsync(ScheduleCommand(secondSection, secondInstructor, baseData.ClassroomId, "10:30", "11:30"));
        var instructorConflict = await schedules.CreateAsync(ScheduleCommand(secondSection, baseData.InstructorId, secondClassroom, "10:30", "11:30"));
        var sectionConflict = await schedules.CreateAsync(ScheduleCommand(baseData.SectionId, secondInstructor, secondClassroom, "10:30", "11:30"));

        Assert.False(classroomConflict.Succeeded);
        Assert.Contains(classroomConflict.Errors, error => error.MessageKey == "ErrorScheduleClassroomConflict");
        Assert.False(instructorConflict.Succeeded);
        Assert.Contains(instructorConflict.Errors, error => error.MessageKey == "ErrorScheduleInstructorConflict");
        Assert.False(sectionConflict.Succeeded);
        Assert.Contains(sectionConflict.Errors, error => error.MessageKey == "ErrorScheduleSectionConflict");
        Assert.False(string.IsNullOrWhiteSpace(secondInstructorUserId));
    }

    [Fact]
    public async Task Assigned_instructor_session_lifecycle_secures_code_and_hides_it_from_students()
    {
        using var scope = _factory.Services.CreateScope();
        var timeZoneService = scope.ServiceProvider.GetRequiredService<IApplicationTimeZoneService>();
        var clock = (MutableDateTimeProvider)scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        clock.UtcNow = timeZoneService.ConvertLocalToUtc(new DateOnly(2026, 7, 22), new TimeOnly(9, 55));

        var data = await CreateAcademicScenarioAsync(scope);
        var schedules = scope.ServiceProvider.GetRequiredService<ILectureScheduleService>();
        var schedule = await schedules.CreateAsync(ScheduleCommand(
            data.SectionId,
            data.InstructorId,
            data.ClassroomId,
            "10:00",
            "11:00",
            DayOfWeek.Wednesday));
        Assert.True(schedule.Succeeded, FormatErrors(schedule.Errors));

        var sessions = scope.ServiceProvider.GetRequiredService<ILectureSessionService>();
        var start = await sessions.StartAsync(data.InstructorUserId, new StartLectureSessionCommand { LectureScheduleId = schedule.Value });
        Assert.True(start.Succeeded, FormatErrors(start.Errors));
        Assert.NotNull(start.Value);
        Assert.False(string.IsNullOrWhiteSpace(start.Value!.PlainSessionCode));

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var stored = await dbContext.LectureSessions.AsNoTracking().SingleAsync(item => item.Id == start.Value.LectureSessionId);
        var codeService = scope.ServiceProvider.GetRequiredService<ISessionCodeService>();
        Assert.NotEqual(start.Value.PlainSessionCode, stored.SessionCodeHash);
        Assert.NotEqual(start.Value.PlainSessionCode, stored.ProtectedSessionCode);
        Assert.True(codeService.VerifyHash(start.Value.PlainSessionCode, stored.SessionCodeHash));
        Assert.DoesNotContain(start.Value.PlainSessionCode, stored.ProtectedSessionCode, StringComparison.Ordinal);

        var studentView = await sessions.GetStudentActiveSessionAsync(data.StudentUserId, start.Value.LectureSessionId);
        Assert.NotNull(studentView);
        Assert.False(studentView.HasCodeHash);
        Assert.False(studentView.HasProtectedCode);

        var regenerate = await sessions.RegenerateCodeAsync(data.InstructorUserId, start.Value.LectureSessionId);
        Assert.True(regenerate.Succeeded, FormatErrors(regenerate.Errors));
        var regenerated = await dbContext.LectureSessions.AsNoTracking().SingleAsync(item => item.Id == start.Value.LectureSessionId);
        Assert.False(codeService.VerifyHash(start.Value.PlainSessionCode, regenerated.SessionCodeHash));
        Assert.True(codeService.VerifyHash(regenerate.Value!.PlainSessionCode, regenerated.SessionCodeHash));

        var sessionDto = await sessions.GetByIdAsync(start.Value.LectureSessionId, includeSensitiveCodeState: true);
        var end = await sessions.EndAsync(
            data.InstructorUserId,
            start.Value.LectureSessionId,
            new EndLectureSessionCommand { RowVersion = sessionDto!.RowVersion, Reason = "Completed" });
        Assert.True(end.Succeeded, FormatErrors(end.Errors));

        var ended = await dbContext.LectureSessions.AsNoTracking().SingleAsync(item => item.Id == start.Value.LectureSessionId);
        var events = await dbContext.LectureSessionEvents.AsNoTracking().Where(item => item.LectureSessionId == start.Value.LectureSessionId).ToListAsync();
        Assert.Equal(LectureSessionStatus.Ended, ended.Status);
        Assert.Equal(string.Empty, ended.SessionCodeHash);
        Assert.Equal(string.Empty, ended.ProtectedSessionCode);
        Assert.Contains(events, item => item.EventType == LectureSessionEventType.Started);
        Assert.Contains(events, item => item.EventType == LectureSessionEventType.CodeRegenerated);
        Assert.Contains(events, item => item.EventType == LectureSessionEventType.Ended);
    }

    [Fact]
    public async Task Instructor_start_outside_configured_window_is_rejected()
    {
        using var scope = _factory.Services.CreateScope();
        var timeZoneService = scope.ServiceProvider.GetRequiredService<IApplicationTimeZoneService>();
        var clock = (MutableDateTimeProvider)scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();
        clock.UtcNow = timeZoneService.ConvertLocalToUtc(new DateOnly(2026, 7, 22), new TimeOnly(8, 0));

        var data = await CreateAcademicScenarioAsync(scope);
        var schedules = scope.ServiceProvider.GetRequiredService<ILectureScheduleService>();
        var schedule = await schedules.CreateAsync(ScheduleCommand(
            data.SectionId,
            data.InstructorId,
            data.ClassroomId,
            "10:00",
            "11:00",
            DayOfWeek.Wednesday));
        Assert.True(schedule.Succeeded, FormatErrors(schedule.Errors));

        var sessions = scope.ServiceProvider.GetRequiredService<ILectureSessionService>();
        var start = await sessions.StartAsync(data.InstructorUserId, new StartLectureSessionCommand { LectureScheduleId = schedule.Value });

        Assert.False(start.Succeeded);
        Assert.Contains(start.Errors, error => error.MessageKey == "ErrorLectureStartWindow");
    }

    private static LectureScheduleCommand ScheduleCommand(
        Guid sectionId,
        Guid instructorId,
        Guid classroomId,
        string start = "10:00",
        string end = "11:00",
        DayOfWeek dayOfWeek = DayOfWeek.Sunday)
        => new()
        {
            SectionId = sectionId,
            InstructorId = instructorId,
            ClassroomId = classroomId,
            DayOfWeek = dayOfWeek,
            StartTime = TimeOnly.Parse(start, CultureInfo.InvariantCulture),
            EndTime = TimeOnly.Parse(end, CultureInfo.InvariantCulture),
            EffectiveFrom = new DateOnly(2026, 7, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            DefaultLateThresholdMinutes = 10,
            DefaultAllowedRadiusMeters = 50
        };

    private static async Task<AcademicScenario> CreateAcademicScenarioAsync(IServiceScope scope)
    {
        var departmentId = await CreateDepartmentAsync(scope);
        var courseId = await CreateCourseAsync(scope, departmentId);
        var sectionId = await CreateSectionAsync(scope, courseId, 40);
        var classroomId = await CreateClassroomAsync(scope, UniqueCode("R"), 100);
        var instructorId = await CreateInstructorAsync(scope, departmentId, "LIN");
        var studentId = await CreateStudentAsync(scope, departmentId, "LST");

        var assignments = scope.ServiceProvider.GetRequiredService<IInstructorAssignmentService>();
        Assert.True((await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = instructorId, SectionId = sectionId, IsPrimary = true })).Succeeded);

        var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        Assert.True((await enrollments.CreateAsync(new EnrollmentCommand { StudentId = studentId, SectionId = sectionId })).Succeeded);

        var instructor = await scope.ServiceProvider.GetRequiredService<IInstructorService>().GetByIdAsync(instructorId);
        var student = await scope.ServiceProvider.GetRequiredService<IStudentService>().GetByIdAsync(studentId);

        return new AcademicScenario(
            departmentId,
            courseId,
            sectionId,
            classroomId,
            instructorId,
            instructor!.ApplicationUserId,
            studentId,
            student!.ApplicationUserId);
    }

    private static async Task<Guid> CreateDepartmentAsync(IServiceScope scope)
    {
        var departments = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
        var code = UniqueCode("LD");
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
        var code = UniqueCode("LC");
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

    private static async Task<Guid> CreateClassroomAsync(IServiceScope scope, string prefix, int capacity)
    {
        var classrooms = scope.ServiceProvider.GetRequiredService<IClassroomService>();
        var result = await classrooms.CreateAsync(new ClassroomCommand
        {
            Code = UniqueCode(prefix),
            BuildingNameEnglish = "Engineering",
            BuildingNameArabic = "الهندسة",
            RoomNumber = UniqueCode("RM"),
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
        => $"{prefix}{Guid.NewGuid():N}"[..Math.Min(prefix.Length + 12, 32)].ToUpperInvariant();

    private static string UniqueEmail(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}@attendai.local";

    private static string FormatErrors(IReadOnlyList<Application.Common.Models.OperationError> errors)
        => string.Join("; ", errors.Select(error => $"{error.Code}:{error.MessageKey}"));

    private sealed record AcademicScenario(
        Guid DepartmentId,
        Guid CourseId,
        Guid SectionId,
        Guid ClassroomId,
        Guid InstructorId,
        string InstructorUserId,
        Guid StudentId,
        string StudentUserId);
}

public sealed class LectureSchedulingWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    public MutableDateTimeProvider Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDateTimeProvider>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));
            services.AddSingleton<IDateTimeProvider>(Clock);
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}

public sealed class MutableDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
}
