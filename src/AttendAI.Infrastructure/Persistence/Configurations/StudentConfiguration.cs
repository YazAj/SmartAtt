using AttendAI.Domain.Academic;
using AttendAI.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");
        builder.HasKey(student => student.Id);

        builder.Property(student => student.ApplicationUserId).HasMaxLength(450).IsRequired();
        builder.Property(student => student.StudentNumber).HasMaxLength(32).IsRequired();
        builder.Property(student => student.NameEnglish).HasMaxLength(160).IsRequired();
        builder.Property(student => student.NameArabic).HasMaxLength(160).IsRequired();
        builder.Property(student => student.AcademicLevel).HasMaxLength(60).IsRequired();
        builder.Property(student => student.IsActive).HasDefaultValue(true);
        builder.Property(student => student.RowVersion).IsRowVersion();

        builder.HasIndex(student => student.StudentNumber).IsUnique();
        builder.HasIndex(student => student.ApplicationUserId).IsUnique();
        builder.HasIndex(student => new { student.DepartmentId, student.AcademicLevel, student.IsActive });

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Student>(student => student.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(student => student.Enrollments)
            .WithOne(enrollment => enrollment.Student)
            .HasForeignKey(enrollment => enrollment.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
