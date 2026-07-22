using AttendAI.Application.FaceRecognition;
using AttendAI.Application.Academic;
using AttendAI.Application.Academic.Dashboard;
using AttendAI.Application.Lectures;
using AttendAI.Infrastructure.Academic;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using AttendAI.Infrastructure.Identity;
using AttendAI.Infrastructure.Lectures;
using AttendAI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AttendAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var faceRecognitionSection = configuration.GetSection(FaceRecognitionOptions.SectionName);
        services.Configure<FaceRecognitionOptions>(options =>
        {
            options.Provider = faceRecognitionSection["Provider"] ?? options.Provider;
            options.ModelPath = faceRecognitionSection["ModelPath"] ?? options.ModelPath;

            if (double.TryParse(faceRecognitionSection["Threshold"], out var threshold))
            {
                options.Threshold = threshold;
            }
        });

        services.Configure<LectureSchedulingOptions>(
            configuration.GetSection(LectureSchedulingOptions.SectionName));

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
        services.AddScoped<IFaceRecognitionEngine, FakeFaceRecognitionEngine>();
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

        return services;
    }
}
