using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Courses_CreditHours", "[CreditHours] BETWEEN 1 AND 6");
        });
        builder.HasKey(course => course.Id);

        builder.Property(course => course.Code).HasMaxLength(32).IsRequired();
        builder.Property(course => course.NameEnglish).HasMaxLength(180).IsRequired();
        builder.Property(course => course.NameArabic).HasMaxLength(180).IsRequired();
        builder.Property(course => course.DescriptionEnglish).HasMaxLength(500);
        builder.Property(course => course.DescriptionArabic).HasMaxLength(500);
        builder.Property(course => course.IsActive).HasDefaultValue(true);
        builder.Property(course => course.RowVersion).IsRowVersion();

        builder.HasIndex(course => course.Code).IsUnique();
        builder.HasIndex(course => new { course.DepartmentId, course.IsActive });

        builder.HasMany(course => course.Sections)
            .WithOne(section => section.Course)
            .HasForeignKey(section => section.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
