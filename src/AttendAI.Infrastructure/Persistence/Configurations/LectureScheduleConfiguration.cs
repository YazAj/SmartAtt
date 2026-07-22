using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class LectureScheduleConfiguration : IEntityTypeConfiguration<LectureSchedule>
{
    public void Configure(EntityTypeBuilder<LectureSchedule> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_LectureSchedules_TimeRange", "[StartTime] < [EndTime]");
            table.HasCheckConstraint("CK_LectureSchedules_EffectiveRange", "[EffectiveFrom] <= [EffectiveTo]");
            table.HasCheckConstraint("CK_LectureSchedules_LateThreshold", "[DefaultLateThresholdMinutes] >= 0");
            table.HasCheckConstraint("CK_LectureSchedules_AllowedRadius", "[DefaultAllowedRadiusMeters] >= 0");
        });

        builder.HasKey(schedule => schedule.Id);

        builder.Property(schedule => schedule.DayOfWeek).HasConversion<int>().IsRequired();
        builder.Property(schedule => schedule.StartTime).IsRequired();
        builder.Property(schedule => schedule.EndTime).IsRequired();
        builder.Property(schedule => schedule.EffectiveFrom).IsRequired();
        builder.Property(schedule => schedule.EffectiveTo).IsRequired();
        builder.Property(schedule => schedule.DefaultLateThresholdMinutes).IsRequired();
        builder.Property(schedule => schedule.DefaultAllowedRadiusMeters).IsRequired();
        builder.Property(schedule => schedule.IsActive).HasDefaultValue(true);
        builder.Property(schedule => schedule.RowVersion).IsRowVersion();

        builder.HasIndex(schedule => new
        {
            schedule.ClassroomId,
            schedule.DayOfWeek,
            schedule.IsActive,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.StartTime,
            schedule.EndTime
        });

        builder.HasIndex(schedule => new
        {
            schedule.InstructorId,
            schedule.DayOfWeek,
            schedule.IsActive,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.StartTime,
            schedule.EndTime
        });

        builder.HasIndex(schedule => new
        {
            schedule.SectionId,
            schedule.DayOfWeek,
            schedule.IsActive,
            schedule.EffectiveFrom,
            schedule.EffectiveTo,
            schedule.StartTime,
            schedule.EndTime
        });

        builder.HasOne(schedule => schedule.Section)
            .WithMany()
            .HasForeignKey(schedule => schedule.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(schedule => schedule.Instructor)
            .WithMany()
            .HasForeignKey(schedule => schedule.InstructorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(schedule => schedule.Classroom)
            .WithMany()
            .HasForeignKey(schedule => schedule.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(schedule => schedule.Sessions)
            .WithOne(session => session.LectureSchedule)
            .HasForeignKey(session => session.LectureScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
