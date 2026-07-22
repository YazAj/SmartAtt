using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("StudentEnrollments");
        builder.HasKey(enrollment => enrollment.Id);

        builder.Property(enrollment => enrollment.EnrollmentStatus).HasConversion<int>().IsRequired();
        builder.Property(enrollment => enrollment.EnrolledAtUtc).IsRequired();
        builder.Property(enrollment => enrollment.IsActive).HasDefaultValue(true);
        builder.Property(enrollment => enrollment.RowVersion).IsRowVersion();

        builder.HasIndex(enrollment => new { enrollment.StudentId, enrollment.SectionId }).IsUnique();
        builder.HasIndex(enrollment => new { enrollment.SectionId, enrollment.EnrollmentStatus, enrollment.IsActive });
    }
}
