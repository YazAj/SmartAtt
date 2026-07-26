using AttendAI.Application.Academic;
using AttendAI.Application.Academic.Dashboard;
using AttendAI.Application.Attendance;
using AttendAI.Application.Biometrics;
using AttendAI.Application.FaceRecognition;
using AttendAI.Application.FaceVerification;
using AttendAI.Application.Lectures;
using AttendAI.Application.Reporting;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Attendance;
using AttendAI.Infrastructure.Biometrics;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using AttendAI.Infrastructure.FaceVerification;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Lectures;
using AttendAI.Infrastructure.Persistence;
using AttendAI.Infrastructure.Reporting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FaceRecognitionOptions>(
            configuration.GetSection(FaceRecognitionOptions.SectionName));

        services.Configure<BiometricEnrollmentOptions>(
            configuration.GetSection(BiometricEnrollmentOptions.SectionName));

        services.Configure<FaceVerificationOptions>(
            configuration.GetSection(FaceVerificationOptions.SectionName));

        services.Configure<LectureSchedulingOptions>(
            configuration.GetSection(LectureSchedulingOptions.SectionName));

        services.Configure<AttendanceOptions>(
            configuration.GetSection(AttendanceOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=AttendAI;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddDataProtection();

        services.AddScoped<IdentitySeedService>();
        services.AddScoped<ApplicationDbInitializer>();
        services.AddSingleton<RealFaceRecognitionModelStore>();
        services.AddScoped<IFaceRecognitionEngine>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FaceRecognitionOptions>>().Value;
            if (options.Provider.Equals("Fake", StringComparison.OrdinalIgnoreCase))
            {
                return ActivatorUtilities.CreateInstance<FakeFaceRecognitionEngine>(serviceProvider);
            }

            if (options.Provider.Equals("Real", StringComparison.OrdinalIgnoreCase))
            {
                return ActivatorUtilities.CreateInstance<OpenCvSFaceRecognitionEngine>(serviceProvider);
            }

            return ActivatorUtilities.CreateInstance<DisabledFaceRecognitionEngine>(serviceProvider);
        });
        services.AddScoped<IFaceCaptureValidator, FaceCaptureValidator>();
        services.AddScoped<IFaceEnrollmentProcessor, FaceEnrollmentProcessor>();
        services.AddScoped<IBiometricTemplateProtector, BiometricTemplateProtector>();
        services.AddSingleton<IBiometricEnrollmentRateLimiter, InMemoryBiometricEnrollmentRateLimiter>();
        services.AddScoped<IFaceEngineReadinessService, FaceEngineReadinessService>();
        services.AddScoped<IBiometricEnrollmentService, BiometricEnrollmentService>();
        services.AddScoped<IFaceEngineDiagnosticsService, FaceEngineDiagnosticsService>();
        services.AddScoped<ITemplateCompatibilityService, TemplateCompatibilityService>();
        services.AddScoped<IFaceVerificationPolicyService, FaceVerificationPolicyService>();
        services.AddSingleton<IFaceVerificationRateLimiter, InMemoryFaceVerificationRateLimiter>();
        services.AddScoped<IFaceVerificationEligibilityService, FaceVerificationEligibilityService>();
        services.AddScoped<IOneToOneFaceVerifier, OneToOneFaceVerifier>();
        services.AddScoped<IFaceVerificationService, FaceVerificationService>();
        services.AddScoped<IFaceVerificationQueryService, FaceVerificationQueryService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IInstructorService, InstructorService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IInstructorAssignmentService, InstructorAssignmentService>();
        services.AddScoped<IAcademicLookupService, AcademicLookupService>();
        services.AddScoped<IAcademicDashboardService, AcademicDashboardService>();
        services.AddScoped<IApplicationTimeZoneService, ApplicationTimeZoneService>();
        services.AddScoped<ISessionCodeService, SessionCodeService>();
        services.AddScoped<ILectureLookupService, LectureLookupService>();
        services.AddScoped<ILectureScheduleService, LectureScheduleService>();
        services.AddScoped<ILectureSessionService, LectureSessionService>();
        services.AddScoped<ILocationVerificationService, LocationVerificationService>();
        services.AddSingleton<IAttendanceRateLimiter, InMemoryAttendanceRateLimiter>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAttendanceReportingService, AttendanceReportingService>();
        services.AddScoped<IAttendanceReportExportService, AttendanceReportingService>();

        return services;
    }
}
