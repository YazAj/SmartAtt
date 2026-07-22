using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class FaceVerificationAttemptConfiguration : IEntityTypeConfiguration<FaceVerificationAttempt>
{
    public void Configure(EntityTypeBuilder<FaceVerificationAttempt> builder)
    {
        builder.ToTable("FaceVerificationAttempts", table =>
        {
            table.HasCheckConstraint("CK_FaceVerificationAttempts_Score", "[Score] IS NULL OR [Score] >= 0");
            table.HasCheckConstraint("CK_FaceVerificationAttempts_Threshold", "[Threshold] >= 0");
            table.HasCheckConstraint("CK_FaceVerificationAttempts_ImageWidth", "[ImageWidth] IS NULL OR [ImageWidth] >= 0");
            table.HasCheckConstraint("CK_FaceVerificationAttempts_ImageHeight", "[ImageHeight] IS NULL OR [ImageHeight] >= 0");
            table.HasCheckConstraint("CK_FaceVerificationAttempts_DetectedFaceCount", "[DetectedFaceCount] IS NULL OR [DetectedFaceCount] >= 0");
            table.HasCheckConstraint("CK_FaceVerificationAttempts_QualityScore", "[QualityScore] IS NULL OR ([QualityScore] >= 0 AND [QualityScore] <= 1)");
            table.HasCheckConstraint("CK_FaceVerificationAttempts_ProcessingDuration", "[ProcessingDurationMilliseconds] IS NULL OR [ProcessingDurationMilliseconds] >= 0");
        });

        builder.HasKey(attempt => attempt.Id);

        builder.Property(attempt => attempt.VerificationPurpose).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.Outcome).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.Decision).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.ErrorCode).HasMaxLength(80).IsRequired();
        builder.Property(attempt => attempt.Score).HasColumnType("decimal(9,6)");
        builder.Property(attempt => attempt.Threshold).HasColumnType("decimal(9,6)");
        builder.Property(attempt => attempt.ScoreMetric).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.EngineName).HasMaxLength(80).IsRequired();
        builder.Property(attempt => attempt.EngineVersion).HasMaxLength(80).IsRequired();
        builder.Property(attempt => attempt.ModelName).HasMaxLength(120).IsRequired();
        builder.Property(attempt => attempt.ModelVersion).HasMaxLength(80).IsRequired();
        builder.Property(attempt => attempt.TemplateFormatVersion).HasMaxLength(80).IsRequired();
        builder.Property(attempt => attempt.ClientRequestId).HasMaxLength(80).IsRequired();
        builder.Property(attempt => attempt.SafeDescription).HasMaxLength(300).IsRequired();
        builder.Property(attempt => attempt.QualityScore).HasColumnType("decimal(5,4)");

        builder.HasIndex(attempt => new { attempt.StudentId, attempt.AttemptedAtUtc })
            .HasDatabaseName("IX_FaceVerificationAttempts_StudentId_AttemptedAtUtc");

        builder.HasIndex(attempt => attempt.FaceTemplateId)
            .HasDatabaseName("IX_FaceVerificationAttempts_FaceTemplateId");

        builder.HasIndex(attempt => new { attempt.Outcome, attempt.AttemptedAtUtc })
            .HasDatabaseName("IX_FaceVerificationAttempts_Outcome_AttemptedAtUtc");

        builder.HasIndex(attempt => new { attempt.StudentId, attempt.ClientRequestId })
            .IsUnique()
            .HasFilter("[ClientRequestId] <> ''")
            .HasDatabaseName("UX_FaceVerificationAttempts_StudentId_ClientRequestId");

        builder.HasOne(attempt => attempt.Student)
            .WithMany()
            .HasForeignKey(attempt => attempt.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(attempt => attempt.FaceTemplate)
            .WithMany()
            .HasForeignKey(attempt => attempt.FaceTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
