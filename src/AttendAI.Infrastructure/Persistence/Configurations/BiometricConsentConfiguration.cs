using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class BiometricConsentConfiguration : IEntityTypeConfiguration<BiometricConsent>
{
    public void Configure(EntityTypeBuilder<BiometricConsent> builder)
    {
        builder.ToTable("BiometricConsents");
        builder.HasKey(consent => consent.Id);

        builder.Property(consent => consent.ConsentVersion).HasMaxLength(40).IsRequired();
        builder.Property(consent => consent.ConsentTextHash).HasMaxLength(128).IsRequired();
        builder.Property(consent => consent.AcceptedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(consent => consent.WithdrawnByUserId).HasMaxLength(450);
        builder.Property(consent => consent.IsActive).HasDefaultValue(true);
        builder.Property(consent => consent.RowVersion).IsRowVersion();

        builder.HasIndex(consent => consent.StudentId)
            .HasDatabaseName("IX_BiometricConsents_StudentId");

        builder.HasIndex(consent => consent.StudentId)
            .IsUnique()
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName("UX_BiometricConsents_StudentId_Active");

        builder.HasOne(consent => consent.Student)
            .WithMany()
            .HasForeignKey(consent => consent.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
