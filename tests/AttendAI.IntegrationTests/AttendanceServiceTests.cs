using System.Globalization;
using System.Text;
using AttendAI.Application.Academic;
using AttendAI.Application.Attendance;
using AttendAI.Application.Biometrics;
using AttendAI.Application.Common.Interfaces;
using AttendAI.Application.FaceVerification;
using AttendAI.Application.Lectures;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Persistence;
using AttendAI.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AttendAI.IntegrationTests;

public sealed class AttendanceServiceTests : IClassFixture<AttendanceWebApplicationFactory>
{
    private const string Password = "Temp!12345A";
    private readonly AttendanceWebApplicationFactory _factory;

    public AttendanceServiceTests(AttendanceWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Check_in_with_matching_real_ready_face_and_location_creates_attendance_record()
    {
        using var scope = _factory.Services.CreateScope();
        _factory.Diagnostics.Mode = "Real";
        var scenario = await CreateScenarioAsync(scope);
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();

        var challenge = await attendance.IssueChallengeAsync(scenario.StudentUserId, scenario.LectureSessionId);
        Assert.True(challenge.Succeeded, FormatErrors(challenge.Errors));

        var result = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(scenario.LectureSessionId, challenge.Value!.ChallengeToken, "success", "MATCH", 31.9539, 35.9106));

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        Assert.True(result.Value!.Succeeded);
        Assert.Equal(AttendanceStatus.Present, result.Value.Status);
        Assert.NotNull(result.Value.AttendanceRecordId);
        Assert.Equal(FaceVerificationDecision.Match, result.Value.FaceDecision);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await dbContext.AttendanceRecords.CountAsync(item => item.StudentId == scenario.StudentId && item.LectureSessionId == scenario.LectureSessionId));
        Assert.Equal(1, await dbContext.FaceVerificationAttempts.CountAsync(item => item.StudentId == scenario.StudentId && item.VerificationPurpose == FaceVerificationPurpose.FutureAttendance));

        var roster = await attendance.GetInstructorRosterAsync(scenario.InstructorUserId, scenario.LectureSessionId);
        Assert.NotNull(roster);
        Assert.Equal(1, roster!.PresentCount);
        Assert.Equal(0, roster.MissingCount);
        Assert.Equal(AttendanceStatus.Present, roster.Rows.Single().Status);
    }

    [Fact]
    public async Task Duplicate_check_in_does_not_create_second_record()
    {
        using var scope = _factory.Services.CreateScope();
        _factory.Diagnostics.Mode = "Real";
        var scenario = await CreateScenarioAsync(scope);
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();

        var firstChallenge = await attendance.IssueChallengeAsync(scenario.StudentUserId, scenario.LectureSessionId);
        var first = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(scenario.LectureSessionId, firstChallenge.Value!.ChallengeToken, "duplicate-1", "MATCH", 31.9539, 35.9106));
        Assert.True(first.Succeeded, FormatErrors(first.Errors));

        var secondChallenge = await attendance.IssueChallengeAsync(scenario.StudentUserId, scenario.LectureSessionId);
        Assert.False(secondChallenge.Succeeded);
        Assert.Contains(secondChallenge.Errors, error => error.MessageKey == "ErrorAttendanceAlreadyRecorded");

        var second = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(scenario.LectureSessionId, firstChallenge.Value.ChallengeToken, "duplicate-2", "MATCH", 31.9539, 35.9106));
        Assert.True(second.Succeeded, FormatErrors(second.Errors));
        Assert.True(second.Value!.AlreadyRecorded);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(1, await dbContext.AttendanceRecords.CountAsync(item => item.StudentId == scenario.StudentId && item.LectureSessionId == scenario.LectureSessionId));
    }

    [Fact]
    public async Task Different_person_face_result_is_rejected_without_attendance_record()
    {
        using var scope = _factory.Services.CreateScope();
        _factory.Diagnostics.Mode = "Real";
        var scenario = await CreateScenarioAsync(scope);
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
        var challenge = await attendance.IssueChallengeAsync(scenario.StudentUserId, scenario.LectureSessionId);

        var result = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(scenario.LectureSessionId, challenge.Value!.ChallengeToken, "different-person", "NO_MATCH", 31.9539, 35.9106));

        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        Assert.False(result.Value!.Succeeded);
        Assert.Equal(AttendanceFailureReason.FaceNotMatched, result.Value.FailureReason);

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(0, await dbContext.AttendanceRecords.CountAsync(item => item.StudentId == scenario.StudentId && item.LectureSessionId == scenario.LectureSessionId));
    }

    [Fact]
    public async Task Outside_geofence_is_rejected_and_consumes_challenge()
    {
        using var scope = _factory.Services.CreateScope();
        _factory.Diagnostics.Mode = "Real";
        var scenario = await CreateScenarioAsync(scope);
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
        var challenge = await attendance.IssueChallengeAsync(scenario.StudentUserId, scenario.LectureSessionId);

        var outside = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(scenario.LectureSessionId, challenge.Value!.ChallengeToken, "outside", "MATCH", 32.0000, 35.9106));
        var replay = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(scenario.LectureSessionId, challenge.Value.ChallengeToken, "outside-replay", "MATCH", 31.9539, 35.9106));

        Assert.True(outside.Succeeded, FormatErrors(outside.Errors));
        Assert.Equal(AttendanceFailureReason.OutsideGeofence, outside.Value!.FailureReason);
        Assert.True(replay.Succeeded, FormatErrors(replay.Errors));
        Assert.Equal(AttendanceFailureReason.ChallengeAlreadyUsed, replay.Value!.FailureReason);
    }

    [Fact]
    public async Task Attendance_requires_real_face_engine_even_when_fake_engine_can_verify_elsewhere()
    {
        using var scope = _factory.Services.CreateScope();
        _factory.Diagnostics.Mode = "Fake";
        var scenario = await CreateScenarioAsync(scope);
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();

        var challenge = await attendance.IssueChallengeAsync(scenario.StudentUserId, scenario.LectureSessionId);

        Assert.False(challenge.Succeeded);
        Assert.Contains(challenge.Errors, error => error.MessageKey == "ErrorAttendanceRealFaceEngineRequired");
    }

    [Fact]
    public async Task Check_in_for_missing_session_returns_not_found_without_attempt()
    {
        using var scope = _factory.Services.CreateScope();
        _factory.Diagnostics.Mode = "Real";
        var scenario = await CreateScenarioAsync(scope);
        var attendance = scope.ServiceProvider.GetRequiredService<IAttendanceService>();
        var missingSessionId = Guid.NewGuid();

        var result = await attendance.CheckInAsync(
            scenario.StudentUserId,
            Command(missingSessionId, "missing-session", "missing-session", "MATCH", 31.9539, 35.9106));

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.MessageKey == "ErrorLectureSessionNotFound");

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(0, await dbContext.AttendanceAttempts.CountAsync(item => item.LectureSessionId == missingSessionId));
    }

    private static AttendanceCheckInCommand Command(
        Guid lectureSessionId,
        string challengeToken,
        string idempotencyKey,
        string marker,
        double latitude,
        double longitude)
        => new(
            lectureSessionId,
            challengeToken,
            idempotencyKey,
            Sample(marker),
            new BrowserLocationSample(latitude, longitude, 10, DateTimeOffset.UtcNow));

    private async Task<AttendanceScenario> CreateScenarioAsync(IServiceScope scope)
    {
        var timeZoneService = scope.ServiceProvider.GetRequiredService<IApplicationTimeZoneService>();
        _factory.Clock.UtcNow = timeZoneService.ConvertLocalToUtc(new DateOnly(2026, 7, 22), new TimeOnly(9, 55));

        var departmentId = await CreateDepartmentAsync(scope);
        var courseId = await CreateCourseAsync(scope, departmentId);
        var sectionId = await CreateSectionAsync(scope, courseId);
        var classroomId = await CreateClassroomAsync(scope);
        var instructorId = await CreateInstructorAsync(scope, departmentId);
        var studentId = await CreateStudentAsync(scope, departmentId);

        var assignments = scope.ServiceProvider.GetRequiredService<IInstructorAssignmentService>();
        Assert.True((await assignments.CreateAsync(new InstructorAssignmentCommand { InstructorId = instructorId, SectionId = sectionId, IsPrimary = true })).Succeeded);
        var enrollments = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
        Assert.True((await enrollments.CreateAsync(new EnrollmentCommand { StudentId = studentId, SectionId = sectionId })).Succeeded);

        var instructors = scope.ServiceProvider.GetRequiredService<IInstructorService>();
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var instructor = await instructors.GetByIdAsync(instructorId);
        var student = await students.GetByIdAsync(studentId);
        Assert.NotNull(instructor);
        Assert.NotNull(student);

        await ClearPasswordChangeFlagAsync(scope, instructor!.ApplicationUserId);
        await ClearPasswordChangeFlagAsync(scope, student!.ApplicationUserId);

        var schedules = scope.ServiceProvider.GetRequiredService<ILectureScheduleService>();
        var schedule = await schedules.CreateAsync(new LectureScheduleCommand
        {
            SectionId = sectionId,
            InstructorId = instructorId,
            ClassroomId = classroomId,
            DayOfWeek = DayOfWeek.Wednesday,
            StartTime = TimeOnly.Parse("10:00", CultureInfo.InvariantCulture),
            EndTime = TimeOnly.Parse("11:00", CultureInfo.InvariantCulture),
            EffectiveFrom = new DateOnly(2026, 7, 1),
            EffectiveTo = new DateOnly(2026, 12, 31),
            DefaultLateThresholdMinutes = 10,
            DefaultAllowedRadiusMeters = 50
        });
        Assert.True(schedule.Succeeded, FormatErrors(schedule.Errors));

        var sessions = scope.ServiceProvider.GetRequiredService<ILectureSessionService>();
        var start = await sessions.StartAsync(instructor.ApplicationUserId, new StartLectureSessionCommand { LectureScheduleId = schedule.Value });
        Assert.True(start.Succeeded, FormatErrors(start.Errors));

        return new AttendanceScenario(
            departmentId,
            courseId,
            sectionId,
            classroomId,
            instructorId,
            instructor.ApplicationUserId,
            studentId,
            student.ApplicationUserId,
            start.Value!.LectureSessionId);
    }

    private static async Task<Guid> CreateDepartmentAsync(IServiceScope scope)
    {
        var departments = scope.ServiceProvider.GetRequiredService<IDepartmentService>();
        var code = UniqueCode("AD");
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
        var code = UniqueCode("AC");
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

    private static async Task<Guid> CreateSectionAsync(IServiceScope scope, Guid courseId)
    {
        var sections = scope.ServiceProvider.GetRequiredService<ISectionService>();
        var result = await sections.CreateAsync(new SectionCommand
        {
            CourseId = courseId,
            SectionNumber = UniqueCode("S"),
            AcademicYear = "2026/2027",
            Semester = Semester.First,
            Capacity = 40
        });
        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateClassroomAsync(IServiceScope scope)
    {
        var classrooms = scope.ServiceProvider.GetRequiredService<IClassroomService>();
        var code = UniqueCode("AR");
        var result = await classrooms.CreateAsync(new ClassroomCommand
        {
            Code = code,
            BuildingNameEnglish = $"Building {code}",
            BuildingNameArabic = $"مبنى {code}",
            RoomNumber = "101",
            Capacity = 60,
            Latitude = 31.9539m,
            Longitude = 35.9106m
        });
        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateInstructorAsync(IServiceScope scope, Guid departmentId)
    {
        var instructors = scope.ServiceProvider.GetRequiredService<IInstructorService>();
        var employeeNumber = UniqueCode("AI");
        var result = await instructors.CreateAccountAsync(new InstructorAccountCommand
        {
            Email = $"{employeeNumber.ToLowerInvariant()}@attendai.test",
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            EmployeeNumber = employeeNumber,
            NameEnglish = "Attendance Instructor",
            NameArabic = "مدرس حضور",
            DepartmentId = departmentId,
            AcademicTitle = "Lecturer"
        });
        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task<Guid> CreateStudentAsync(IServiceScope scope, Guid departmentId)
    {
        var students = scope.ServiceProvider.GetRequiredService<IStudentService>();
        var studentNumber = UniqueCode("AS");
        var result = await students.CreateAccountAsync(new StudentAccountCommand
        {
            Email = $"{studentNumber.ToLowerInvariant()}@attendai.test",
            TemporaryPassword = Password,
            ConfirmTemporaryPassword = Password,
            StudentNumber = studentNumber,
            NameEnglish = "Attendance Student",
            NameArabic = "طالب حضور",
            DepartmentId = departmentId,
            EnrollmentYear = 2026,
            AcademicLevel = "First"
        });
        Assert.True(result.Succeeded, FormatErrors(result.Errors));
        return result.Value;
    }

    private static async Task ClearPasswordChangeFlagAsync(IServiceScope scope, string userId)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        user!.MustChangePassword = false;
        await userManager.UpdateAsync(user);
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
        bytes.AddRange(Encoding.UTF8.GetBytes(marker));
        return bytes.ToArray();
    }

    private static byte[] Int32BigEndian(int value)
        => [(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value];

    private static string UniqueCode(string prefix)
        => $"{prefix}{Guid.NewGuid():N}"[..12].ToUpperInvariant();

    private static string FormatErrors(IEnumerable<Application.Common.Models.OperationError> errors)
        => string.Join("; ", errors.Select(error => $"{error.Code}:{error.MessageKey}"));

    private sealed record AttendanceScenario(
        Guid DepartmentId,
        Guid CourseId,
        Guid SectionId,
        Guid ClassroomId,
        Guid InstructorId,
        string InstructorUserId,
        Guid StudentId,
        string StudentUserId,
        Guid LectureSessionId);
}

public sealed class AttendanceWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private SqliteConnection? _connection;

    public MutableDateTimeProvider Clock { get; } = new();

    public TestFaceEngineDiagnostics Diagnostics { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDateTimeProvider>();
            services.RemoveAll<IFaceEngineDiagnosticsService>();
            services.RemoveAll<IOneToOneFaceVerifier>();

            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));
            services.AddSingleton<IDateTimeProvider>(Clock);
            services.AddSingleton<IFaceEngineDiagnosticsService>(Diagnostics);
            services.AddScoped<IOneToOneFaceVerifier, TestOneToOneFaceVerifier>();
            services.Configure<AttendanceOptions>(options =>
            {
                options.CooldownSeconds = 0;
                options.MaximumAttemptsPerWindow = 100;
                options.ChallengeLifetimeMinutes = 5;
                options.MaximumAcceptedAccuracyMeters = 75;
            });
            services.Configure<FaceVerificationOptions>(options =>
            {
                options.CooldownSeconds = 0;
                options.MaximumAttemptsPerWindow = 100;
            });
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

public sealed class TestFaceEngineDiagnostics : IFaceEngineDiagnosticsService
{
    public string Mode { get; set; } = "Real";

    public FaceEngineDiagnosticsDto GetDiagnostics()
    {
        var verificationEnabled = Mode.Equals("Real", StringComparison.OrdinalIgnoreCase);
        return new FaceEngineDiagnosticsDto(
            Mode,
            Mode.Equals("Real", StringComparison.OrdinalIgnoreCase) ? "OpenCV-SFace" : "Fake",
            "test",
            Mode.Equals("Real", StringComparison.OrdinalIgnoreCase) ? "SFace" : "Fake",
            "test",
            verificationEnabled,
            false,
            verificationEnabled,
            verificationEnabled ? "FaceVerificationReadyMessage" : "FaceVerificationFakeEngineBlockedMessage",
            verificationEnabled ? "Runtime Ready" : "Runtime Blocked",
            0.363m,
            ScoreMetric.CosineSimilarity,
            100,
            10,
            0,
            Mode.Equals("Real", StringComparison.OrdinalIgnoreCase) ? "opencv-sface-f32le-v1" : "fake-sha256-v1",
            128);
    }
}

public sealed class TestOneToOneFaceVerifier : IOneToOneFaceVerifier
{
    public Task<OneToOneVerificationResult> VerifyAsync(
        Guid studentId,
        FaceCaptureSample capture,
        FaceVerificationContext context,
        CancellationToken cancellationToken = default)
    {
        var marker = Encoding.UTF8.GetString(capture.ImageBytes);
        var result = marker switch
        {
            var value when value.Contains("NO_MATCH", StringComparison.Ordinal) => Result(FaceVerificationOutcome.NotMatched, FaceVerificationDecision.NoMatch, 0.12m, "NoMatch"),
            var value when value.Contains("NO_FACE", StringComparison.Ordinal) => Result(FaceVerificationOutcome.NoFace, FaceVerificationDecision.NoFaceDetected, null, "NoFace"),
            var value when value.Contains("MULTIPLE", StringComparison.Ordinal) => Result(FaceVerificationOutcome.MultipleFaces, FaceVerificationDecision.MultipleFacesDetected, null, "MultipleFaces"),
            var value when value.Contains("LOW_QUALITY", StringComparison.Ordinal) => Result(FaceVerificationOutcome.LowQuality, FaceVerificationDecision.EngineError, null, "QualityTooLow", qualityScore: 0.1m),
            _ => Result(FaceVerificationOutcome.Matched, FaceVerificationDecision.Match, 0.95m, string.Empty)
        };

        return Task.FromResult(result);
    }

    private static OneToOneVerificationResult Result(
        FaceVerificationOutcome outcome,
        FaceVerificationDecision decision,
        decimal? score,
        string errorCode,
        decimal? qualityScore = 0.9m)
        => new(
            null,
            outcome,
            decision,
            errorCode,
            outcome == FaceVerificationOutcome.Matched ? "FaceVerificationMatchedMessage" : "ErrorFaceVerificationEligibility",
            score,
            0.363m,
            ScoreMetric.CosineSimilarity,
            "OpenCV-SFace",
            "test",
            "SFace",
            "test",
            "opencv-sface-f32le-v1",
            160,
            120,
            decision == FaceVerificationDecision.MultipleFacesDetected ? 2 : decision == FaceVerificationDecision.NoFaceDetected ? 0 : 1,
            qualityScore,
            1);
}
