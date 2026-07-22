using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class InstructorAssignmentConfiguration : IEntityTypeConfiguration<InstructorAssignment>
{
    public void Configure(EntityTypeBuilder<InstructorAssignment> builder)
    {
        builder.ToTable("InstructorAssignments");
        builder.HasKey(assignment => assignment.Id);

        builder.Property(assignment => assignment.AssignedAtUtc).IsRequired();
        builder.Property(assignment => assignment.IsActive).HasDefaultValue(true);
        builder.Property(assignment => assignment.RowVersion).IsRowVersion();

        builder.HasIndex(assignment => new { assignment.InstructorId, assignment.SectionId }).IsUnique();
        builder.HasIndex(assignment => new { assignment.SectionId, assignment.IsPrimary, assignment.IsActive })
            .IsUnique()
            .HasFilter("[IsPrimary] = 1 AND [IsActive] = 1");
    }
}
