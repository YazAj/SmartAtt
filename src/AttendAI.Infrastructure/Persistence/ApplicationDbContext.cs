using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AttendAI.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Instructor> Instructors => Set<Instructor>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Section> Sections => Set<Section>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();

    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();

    public DbSet<InstructorAssignment> InstructorAssignments => Set<InstructorAssignment>();

    public DbSet<LectureSchedule> LectureSchedules => Set<LectureSchedule>();

    public DbSet<LectureSession> LectureSessions => Set<LectureSession>();

    public DbSet<LectureSessionEvent> LectureSessionEvents => Set<LectureSessionEvent>();

    public DbSet<BiometricConsent> BiometricConsents => Set<BiometricConsent>();

    public DbSet<StudentFaceTemplate> StudentFaceTemplates => Set<StudentFaceTemplate>();

    public DbSet<FaceEnrollmentEvent> FaceEnrollmentEvents => Set<FaceEnrollmentEvent>();

    public DbSet<FaceVerificationAttempt> FaceVerificationAttempts => Set<FaceVerificationAttempt>();

    public DbSet<AttendanceChallenge> AttendanceChallenges => Set<AttendanceChallenge>();

    public DbSet<AttendanceAttempt> AttendanceAttempts => Set<AttendanceAttempt>();

    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(160);

            entity.Property(user => user.IsDisabled)
                .HasDefaultValue(false);

            entity.Property(user => user.IsActive)
                .HasDefaultValue(true);

            entity.Property(user => user.MustChangePassword)
                .HasDefaultValue(false);
        });

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            ConfigureSqliteRowVersion<Department>(builder);
            ConfigureSqliteRowVersion<Student>(builder);
            ConfigureSqliteRowVersion<Instructor>(builder);
            ConfigureSqliteRowVersion<Course>(builder);
            ConfigureSqliteRowVersion<Section>(builder);
            ConfigureSqliteRowVersion<Classroom>(builder);
            ConfigureSqliteRowVersion<StudentEnrollment>(builder);
            ConfigureSqliteRowVersion<InstructorAssignment>(builder);
            ConfigureSqliteRowVersion<LectureSchedule>(builder);
            ConfigureSqliteRowVersion<LectureSession>(builder);
            ConfigureSqliteRowVersion<BiometricConsent>(builder);
            ConfigureSqliteRowVersion<StudentFaceTemplate>(builder);
            ConfigureSqliteRowVersion<AttendanceChallenge>(builder);
            ConfigureSqliteRowVersion<AttendanceRecord>(builder);
        }
    }

    private static void ConfigureSqliteRowVersion<TEntity>(ModelBuilder builder)
        where TEntity : AcademicEntity
        => builder.Entity<TEntity>()
            .Property(entity => entity.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever()
            .HasDefaultValue(Array.Empty<byte>());
}
