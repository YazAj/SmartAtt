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
