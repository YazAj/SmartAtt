using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Sections_Capacity", "[Capacity] > 0");
        });
        builder.HasKey(section => section.Id);

        builder.Property(section => section.SectionNumber).HasMaxLength(32).IsRequired();
        builder.Property(section => section.AcademicYear).HasMaxLength(20).IsRequired();
        builder.Property(section => section.Semester).HasConversion<int>().IsRequired();
        builder.Property(section => section.IsActive).HasDefaultValue(true);
        builder.Property(section => section.RowVersion).IsRowVersion();

        builder.HasIndex(section => new { section.CourseId, section.AcademicYear, section.Semester, section.SectionNumber }).IsUnique();
        builder.HasIndex(section => new { section.CourseId, section.AcademicYear, section.Semester, section.IsActive });

        builder.HasMany(section => section.Enrollments)
            .WithOne(enrollment => enrollment.Section)
            .HasForeignKey(enrollment => enrollment.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(section => section.InstructorAssignments)
            .WithOne(assignment => assignment.Section)
            .HasForeignKey(assignment => assignment.SectionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
