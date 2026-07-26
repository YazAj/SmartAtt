using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class AttendanceChallengeConfiguration : IEntityTypeConfiguration<AttendanceChallenge>
{
    public void Configure(EntityTypeBuilder<AttendanceChallenge> builder)
    {
        builder.ToTable("AttendanceChallenges", table =>
        {
            table.HasCheckConstraint("CK_AttendanceChallenges_ExpiresAfterIssued", "[ExpiresAtUtc] > [IssuedAtUtc]");
        });

        builder.HasKey(challenge => challenge.Id);

        builder.Property(challenge => challenge.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(challenge => challenge.IssuedAtUtc).IsRequired();
        builder.Property(challenge => challenge.ExpiresAtUtc).IsRequired();
        builder.Property(challenge => challenge.RowVersion).IsRowVersion();

        builder.HasIndex(challenge => challenge.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_AttendanceChallenges_TokenHash");

        builder.HasIndex(challenge => new { challenge.StudentId, challenge.LectureSessionId, challenge.ExpiresAtUtc })
            .HasDatabaseName("IX_AttendanceChallenges_Student_Session_Expires");

        builder.HasOne(challenge => challenge.Student)
            .WithMany()
            .HasForeignKey(challenge => challenge.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(challenge => challenge.LectureSession)
            .WithMany()
            .HasForeignKey(challenge => challenge.LectureSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
