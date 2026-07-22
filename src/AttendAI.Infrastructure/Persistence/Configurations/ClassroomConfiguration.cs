using AttendAI.Domain.Academic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AttendAI.Infrastructure.Persistence.Configurations;

public sealed class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Classrooms_Capacity", "[Capacity] > 0");
            table.HasCheckConstraint("CK_Classrooms_CoordinatePair", "(([Latitude] IS NULL AND [Longitude] IS NULL) OR ([Latitude] IS NOT NULL AND [Longitude] IS NOT NULL))");
            table.HasCheckConstraint("CK_Classrooms_Latitude", "[Latitude] IS NULL OR ([Latitude] BETWEEN -90 AND 90)");
            table.HasCheckConstraint("CK_Classrooms_Longitude", "[Longitude] IS NULL OR ([Longitude] BETWEEN -180 AND 180)");
        });
        builder.HasKey(classroom => classroom.Id);

        builder.Property(classroom => classroom.Code).HasMaxLength(32).IsRequired();
        builder.Property(classroom => classroom.BuildingNameEnglish).HasMaxLength(160).IsRequired();
        builder.Property(classroom => classroom.BuildingNameArabic).HasMaxLength(160).IsRequired();
        builder.Property(classroom => classroom.RoomNumber).HasMaxLength(60).IsRequired();
        builder.Property(classroom => classroom.Latitude).HasPrecision(9, 6);
        builder.Property(classroom => classroom.Longitude).HasPrecision(9, 6);
        builder.Property(classroom => classroom.IsActive).HasDefaultValue(true);
        builder.Property(classroom => classroom.RowVersion).IsRowVersion();

        builder.HasIndex(classroom => classroom.Code).IsUnique();
        builder.HasIndex(classroom => new { classroom.BuildingNameEnglish, classroom.RoomNumber });
    }
}
