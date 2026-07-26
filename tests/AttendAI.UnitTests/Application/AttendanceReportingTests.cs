using System.Text;
using AttendAI.Application.Reporting;
using AttendAI.Domain.Enums;

namespace AttendAI.UnitTests.Application;

public sealed class AttendanceReportingTests
{
    [Theory]
    [InlineData(1, 1, 4, 50)]
    [InlineData(2, 1, 3, 100)]
    [InlineData(0, 0, 0, 0)]
    public void CalculatePercentage_uses_present_and_late_over_eligible_sessions(
        int present,
        int late,
        int eligible,
        decimal expected)
    {
        var actual = AttendanceReportCalculator.CalculatePercentage(present, late, eligible);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculatePercentage_rejects_negative_counts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AttendanceReportCalculator.CalculatePercentage(-1, 0, 1));
    }

    [Fact]
    public void AttendanceCsv_neutralizes_formula_values_and_omits_sensitive_columns()
    {
        var csv = Encoding.UTF8.GetString(SafeCsvReportWriter.BuildAttendanceCsv([
            new AttendanceReportRowDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "S-001",
                "=cmd|' /C calc'!A0",
                "طالب",
                "CSV101",
                "Course, With Comma",
                "مساق",
                "A",
                "Instructor",
                "مدرس",
                "LAB1",
                new DateOnly(2026, 7, 26),
                new DateTimeOffset(2026, 7, 26, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 7, 26, 11, 0, 0, TimeSpan.Zero),
                AttendanceReportStatus.Present,
                AttendanceStatus.Present,
                new DateTimeOffset(2026, 7, 26, 10, 2, 0, TimeSpan.Zero))
        ]));

        Assert.Contains("S-001,'=cmd|' /C calc'!A0", csv);
        Assert.Contains("\"Course, With Comma\"", csv);
        Assert.DoesNotContain("FaceScore", csv, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DistanceMeters", csv, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Latitude", csv, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Template", csv, StringComparison.OrdinalIgnoreCase);
    }
}
