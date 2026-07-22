using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(department => department.Id);

        builder.Property(department => department.Code).HasMaxLength(32).IsRequired();
        builder.Property(department => department.NameEnglish).HasMaxLength(160).IsRequired();
        builder.Property(department => department.NameArabic).HasMaxLength(160).IsRequired();
        builder.Property(department => department.DescriptionEnglish).HasMaxLength(500);
        builder.Property(department => department.DescriptionArabic).HasMaxLength(500);
        builder.Property(department => department.IsActive).HasDefaultValue(true);
        builder.Property(department => department.RowVersion).IsRowVersion();

        builder.HasIndex(department => department.Code).IsUnique();

        builder.HasMany(department => department.Students)
            .WithOne(student => student.Department)
            .HasForeignKey(student => student.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(department => department.Instructors)
            .WithOne(instructor => instructor.Department)
            .HasForeignKey(instructor => instructor.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(department => department.Courses)
            .WithOne(course => course.Department)
            .HasForeignKey(course => course.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
