using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class LectureSessionConfiguration : IEntityTypeConfiguration<LectureSession>
{
    public void Configure(EntityTypeBuilder<LectureSession> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_LectureSessions_ScheduledRange", "[ScheduledStartUtc] < [ScheduledEndUtc]");
            table.HasCheckConstraint("CK_LectureSessions_LateThreshold", "[LateThresholdMinutes] >= 0");
            table.HasCheckConstraint("CK_LectureSessions_AllowedRadius", "[AllowedRadiusMeters] >= 0");
            table.HasCheckConstraint("CK_LectureSessions_AttendanceLatitude", "[AttendanceLatitude] IS NULL OR (CAST([AttendanceLatitude] AS REAL) BETWEEN -90 AND 90)");
            table.HasCheckConstraint("CK_LectureSessions_AttendanceLongitude", "[AttendanceLongitude] IS NULL OR (CAST([AttendanceLongitude] AS REAL) BETWEEN -180 AND 180)");
            table.HasCheckConstraint("CK_LectureSessions_AttendanceCoordinatePair", "([AttendanceLatitude] IS NULL AND [AttendanceLongitude] IS NULL) OR ([AttendanceLatitude] IS NOT NULL AND [AttendanceLongitude] IS NOT NULL)");
            table.HasCheckConstraint("CK_LectureSessions_MaximumAcceptedAccuracyMeters", "[MaximumAcceptedAccuracyMeters] > 0");
        });

        builder.HasKey(session => session.Id);

        builder.Property(session => session.SessionDate).IsRequired();
        builder.Property(session => session.ScheduledStartUtc).IsRequired();
        builder.Property(session => session.ScheduledEndUtc).IsRequired();
        builder.Property(session => session.ActualStartUtc).IsRequired();
        builder.Property(session => session.Status).HasConversion<int>().IsRequired();
        builder.Property(session => session.SessionCodeHash).HasMaxLength(256).IsRequired();
        builder.Property(session => session.ProtectedSessionCode).HasMaxLength(2048).IsRequired();
        builder.Property(session => session.StartedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(session => session.EndedByUserId).HasMaxLength(450);
        builder.Property(session => session.CancelledByUserId).HasMaxLength(450);
        builder.Property(session => session.EndReason).HasMaxLength(300);
        builder.Property(session => session.AttendanceLatitude).HasColumnType("decimal(9,6)");
        builder.Property(session => session.AttendanceLongitude).HasColumnType("decimal(9,6)");
        builder.Property(session => session.MaximumAcceptedAccuracyMeters).HasDefaultValue(75);
        builder.Property(session => session.AttendanceCheckInEnabled).HasDefaultValue(true);
        builder.Property(session => session.LocationVerificationRequired).HasDefaultValue(true);
        builder.Property(session => session.FaceVerificationRequired).HasDefaultValue(true);
        builder.Property(session => session.RowVersion).IsRowVersion();

        builder.HasIndex(session => new { session.LectureScheduleId, session.SessionDate }).IsUnique();
        builder.HasIndex(session => new { session.SectionId, session.Status });
        builder.HasIndex(session => new { session.InstructorId, session.Status });
        builder.HasIndex(session => new { session.ClassroomId, session.Status });
        builder.HasIndex(session => session.SessionCodeExpiresAtUtc);

        builder.HasOne(session => session.Section)
            .WithMany()
            .HasForeignKey(session => session.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(session => session.Instructor)
            .WithMany()
            .HasForeignKey(session => session.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(session => session.Classroom)
            .WithMany()
            .HasForeignKey(session => session.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(session => session.Events)
            .WithOne(@event => @event.LectureSession)
            .HasForeignKey(@event => @event.LectureSessionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
