using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class FaceEnrollmentEventConfiguration : IEntityTypeConfiguration<FaceEnrollmentEvent>
{
    public void Configure(EntityTypeBuilder<FaceEnrollmentEvent> builder)
    {
        builder.ToTable("FaceEnrollmentEvents");
        builder.HasKey(@event => @event.Id);

        builder.Property(@event => @event.EventType).HasConversion<int>().IsRequired();
        builder.Property(@event => @event.Outcome).HasMaxLength(40).IsRequired();
        builder.Property(@event => @event.ErrorCode).HasMaxLength(80).IsRequired();
        builder.Property(@event => @event.PerformedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(@event => @event.EngineName).HasMaxLength(80).IsRequired();
        builder.Property(@event => @event.EngineVersion).HasMaxLength(80).IsRequired();
        builder.Property(@event => @event.SafeDescription).HasMaxLength(300).IsRequired();

        builder.HasIndex(@event => new { @event.StudentId, @event.OccurredAtUtc })
            .HasDatabaseName("IX_FaceEnrollmentEvents_StudentId_OccurredAtUtc");

        builder.HasIndex(@event => @event.FaceTemplateId)
            .HasDatabaseName("IX_FaceEnrollmentEvents_FaceTemplateId");

        builder.HasIndex(@event => @event.BiometricConsentId)
            .HasDatabaseName("IX_FaceEnrollmentEvents_BiometricConsentId");

        builder.HasOne(@event => @event.Student)
            .WithMany()
            .HasForeignKey(@event => @event.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(@event => @event.FaceTemplate)
            .WithMany()
            .HasForeignKey(@event => @event.FaceTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(@event => @event.BiometricConsent)
            .WithMany()
            .HasForeignKey(@event => @event.BiometricConsentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
