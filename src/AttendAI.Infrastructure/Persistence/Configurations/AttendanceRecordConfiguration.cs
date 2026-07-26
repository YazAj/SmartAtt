using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords", table =>
        {
            table.HasCheckConstraint("CK_AttendanceRecords_AllowedRadiusMeters", "[AllowedRadiusMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceRecords_MaximumAcceptedAccuracyMeters", "[MaximumAcceptedAccuracyMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceRecords_DistanceMeters", "[DistanceMeters] >= 0");
            table.HasCheckConstraint("CK_AttendanceRecords_BrowserAccuracyMeters", "[BrowserAccuracyMeters] >= 0");
        });

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Status).HasConversion<int>().IsRequired();
        builder.Property(record => record.CheckedInAtUtc).IsRequired();
        builder.Property(record => record.DistanceMeters).HasColumnType("decimal(9,3)");
        builder.Property(record => record.BrowserAccuracyMeters).HasColumnType("decimal(9,3)");
        builder.Property(record => record.RowVersion).IsRowVersion();

        builder.HasIndex(record => new { record.LectureSessionId, record.StudentId })
            .IsUnique()
            .HasDatabaseName("UX_AttendanceRecords_Session_Student");

        builder.HasIndex(record => new { record.LectureSessionId, record.Status })
            .HasDatabaseName("IX_AttendanceRecords_Session_Status");

        builder.HasIndex(record => record.AttendanceAttemptId)
            .IsUnique()
            .HasDatabaseName("UX_AttendanceRecords_AttendanceAttemptId");

        builder.HasIndex(record => record.FaceVerificationAttemptId)
            .IsUnique()
            .HasDatabaseName("UX_AttendanceRecords_FaceVerificationAttemptId");

        builder.HasOne(record => record.LectureSession)
            .WithMany()
            .HasForeignKey(record => record.LectureSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(record => record.Student)
            .WithMany()
            .HasForeignKey(record => record.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(record => record.AttendanceAttempt)
            .WithMany()
            .HasForeignKey(record => record.AttendanceAttemptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(record => record.FaceVerificationAttempt)
            .WithMany()
            .HasForeignKey(record => record.FaceVerificationAttemptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(record => record.Classroom)
            .WithMany()
            .HasForeignKey(record => record.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
