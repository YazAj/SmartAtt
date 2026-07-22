using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class StudentFaceTemplateConfiguration : IEntityTypeConfiguration<StudentFaceTemplate>
{
    public void Configure(EntityTypeBuilder<StudentFaceTemplate> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_StudentFaceTemplates_EmbeddingDimension", "[EmbeddingDimension] > 0");
            table.HasCheckConstraint("CK_StudentFaceTemplates_QualityScore", "[QualityScore] >= 0 AND [QualityScore] <= 1");
            table.HasCheckConstraint("CK_StudentFaceTemplates_CaptureCount", "[CaptureCount] > 0");
            table.HasCheckConstraint("CK_StudentFaceTemplates_TemplateVersion", "[TemplateVersion] > 0");
        });

        builder.HasKey(template => template.Id);

        builder.Property(template => template.ProtectedTemplate).IsRequired();
        builder.Property(template => template.TemplateFingerprint).HasMaxLength(128).IsRequired();
        builder.Property(template => template.EngineName).HasMaxLength(80).IsRequired();
        builder.Property(template => template.EngineVersion).HasMaxLength(80).IsRequired();
        builder.Property(template => template.ModelName).HasMaxLength(120).IsRequired();
        builder.Property(template => template.ModelVersion).HasMaxLength(80).IsRequired();
        builder.Property(template => template.TemplateFormatVersion).HasMaxLength(80).IsRequired();
        builder.Property(template => template.QualityScore).HasColumnType("decimal(5,4)");
        builder.Property(template => template.RevokedByUserId).HasMaxLength(450);
        builder.Property(template => template.RevocationReason).HasMaxLength(300);
        builder.Property(template => template.RequiresReEnrollment).HasDefaultValue(false);
        builder.Property(template => template.IsActive).HasDefaultValue(true);
        builder.Property(template => template.RowVersion).IsRowVersion();

        builder.HasIndex(template => template.StudentId)
            .IsUnique()
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("UX_StudentFaceTemplates_StudentId_Active");

        builder.HasIndex(template => new { template.StudentId, template.TemplateVersion })
            .IsUnique()
            .HasDatabaseName("UX_StudentFaceTemplates_StudentId_TemplateVersion");

        builder.HasIndex(template => template.TemplateFingerprint)
            .HasDatabaseName("IX_StudentFaceTemplates_TemplateFingerprint");

        builder.HasIndex(template => new { template.StudentId, template.IsActive, template.RequiresReEnrollment })
            .HasDatabaseName("IX_StudentFaceTemplates_Status");

        builder.HasOne(template => template.Student)
            .WithMany()
            .HasForeignKey(template => template.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(template => template.BiometricConsent)
            .WithMany()
            .HasForeignKey(template => template.BiometricConsentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
