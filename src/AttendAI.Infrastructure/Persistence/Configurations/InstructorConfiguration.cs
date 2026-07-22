using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> builder)
    {
        builder.ToTable("Instructors");
        builder.HasKey(instructor => instructor.Id);

        builder.Property(instructor => instructor.ApplicationUserId).HasMaxLength(450).IsRequired();
        builder.Property(instructor => instructor.EmployeeNumber).HasMaxLength(32).IsRequired();
        builder.Property(instructor => instructor.NameEnglish).HasMaxLength(160).IsRequired();
        builder.Property(instructor => instructor.NameArabic).HasMaxLength(160).IsRequired();
        builder.Property(instructor => instructor.AcademicTitle).HasMaxLength(120).IsRequired();
        builder.Property(instructor => instructor.IsActive).HasDefaultValue(true);
        builder.Property(instructor => instructor.RowVersion).IsRowVersion();

        builder.HasIndex(instructor => instructor.EmployeeNumber).IsUnique();
        builder.HasIndex(instructor => instructor.ApplicationUserId).IsUnique();
        builder.HasIndex(instructor => new { instructor.DepartmentId, instructor.AcademicTitle, instructor.IsActive });

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Instructor>(instructor => instructor.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(instructor => instructor.Assignments)
            .WithOne(assignment => assignment.Instructor)
            .HasForeignKey(assignment => assignment.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
