using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using AttendAI.Infrastructure.Identity;
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

        services.AddScoped<IdentitySeedService>();
        services.AddScoped<ApplicationDbInitializer>();
        services.AddScoped<IFaceRecognitionEngine, FakeFaceRecognitionEngine>();

        return services;
    }
}
