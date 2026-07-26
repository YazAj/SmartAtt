namespace AttendAI.Application.Reporting;

public static class AttendanceReportCalculator
{
    public static decimal CalculatePercentage(int presentCount, int lateCount, int eligibleSessionCount)
    {
        if (presentCount < 0 || lateCount < 0 || eligibleSessionCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(eligibleSessionCount), "Attendance counts cannot be negative.");
        }

        if (eligibleSessionCount == 0)
        {
            return 0;
        }

        var attendedCount = presentCount + lateCount;
        return Math.Round(attendedCount * 100m / eligibleSessionCount, 2, MidpointRounding.AwayFromZero);
    }
}
