using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class AttendanceAttemptConfiguration : IEntityTypeConfiguration<AttendanceAttempt>
{
    public void Configure(EntityTypeBuilder<AttendanceAttempt> builder)
    {
        builder.ToTable("AttendanceAttempts", table =>
        {
            table.HasCheckConstraint("CK_AttendanceAttempts_DistanceMeters", "[DistanceMeters] IS NULL OR [DistanceMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceAttempts_BrowserAccuracyMeters", "[BrowserAccuracyMeters] IS NULL OR [BrowserAccuracyMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceAttempts_AllowedRadiusMeters", "[AllowedRadiusMeters] IS NULL OR [AllowedRadiusMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceAttempts_MaximumAcceptedAccuracyMeters", "[MaximumAcceptedAccuracyMeters] IS NULL OR [MaximumAcceptedAccuracyMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceAttempts_ProcessingDuration", "[ProcessingDurationMilliseconds] IS NULL OR [ProcessingDurationMilliseconds] >= 0");
        });

        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.Outcome).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.FailureReason).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.LocationOutcome).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.AttemptedAtUtc).IsRequired();
        builder.Property(attempt => attempt.IdempotencyKeyHash).HasMaxLength(128).IsRequired();
        builder.Property(attempt => attempt.ChallengeTokenHash).HasMaxLength(128).IsRequired();
        builder.Property(attempt => attempt.SafeDescription).HasMaxLength(300).IsRequired();
        builder.Property(attempt => attempt.DistanceMeters).HasColumnType("decimal(9,3)");
        builder.Property(attempt => attempt.BrowserAccuracyMeters).HasColumnType("decimal(9,3)");

        builder.HasIndex(attempt => new { attempt.StudentId, attempt.LectureSessionId, attempt.IdempotencyKeyHash })
            .IsUnique()
            .HasDatabaseName("UX_AttendanceAttempts_Student_Session_Idempotency");

        builder.HasIndex(attempt => new { attempt.LectureSessionId, attempt.AttemptedAtUtc })
            .HasDatabaseName("IX_AttendanceAttempts_Session_AttemptedAtUtc");

        builder.HasIndex(attempt => new { attempt.StudentId, attempt.AttemptedAtUtc })
            .HasDatabaseName("IX_AttendanceAttempts_Student_AttemptedAtUtc");

        builder.HasIndex(attempt => attempt.FaceVerificationAttemptId)
            .HasDatabaseName("IX_AttendanceAttempts_FaceVerificationAttemptId");

        builder.HasOne(attempt => attempt.Student)
            .WithMany()
            .HasForeignKey(attempt => attempt.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(attempt => attempt.LectureSession)
            .WithMany()
            .HasForeignKey(attempt => attempt.LectureSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(attempt => attempt.FaceVerificationAttempt)
            .WithMany()
            .HasForeignKey(attempt => attempt.FaceVerificationAttemptId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
