using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class LectureSessionEventConfiguration : IEntityTypeConfiguration<LectureSessionEvent>
{
    public void Configure(EntityTypeBuilder<LectureSessionEvent> builder)
    {
        builder.HasKey(@event => @event.Id);

        builder.Property(@event => @event.EventType).HasConversion<int>().IsRequired();
        builder.Property(@event => @event.OccurredAtUtc).IsRequired();
        builder.Property(@event => @event.PerformedByUserId).HasMaxLength(450).IsRequired();
        builder.Property(@event => @event.PreviousStatus).HasConversion<int>();
        builder.Property(@event => @event.NewStatus).HasConversion<int>();
        builder.Property(@event => @event.SafeDescription).HasMaxLength(300).IsRequired();

        builder.HasIndex(@event => new { @event.LectureSessionId, @event.OccurredAtUtc });
    }
}
