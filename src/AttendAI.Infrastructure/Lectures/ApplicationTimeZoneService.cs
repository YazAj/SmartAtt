using AttendAI.Application.Lectures;
using Microsoft.Extensions.Options;

namespace AttendAI.Infrastructure.Lectures;

public sealed class ApplicationTimeZoneService : IApplicationTimeZoneService
{
    public ApplicationTimeZoneService(IOptions<LectureSchedulingOptions> options)
    {
        var configuredOptions = options.Value;
        ApplicationTimeZone = ResolveTimeZone(configuredOptions.TimeZoneId, configuredOptions.WindowsTimeZoneId);
    }

    public TimeZoneInfo ApplicationTimeZone { get; }

    public DateTimeOffset ConvertUtcToLocal(DateTimeOffset utc)
        => TimeZoneInfo.ConvertTime(utc, ApplicationTimeZone);

    public DateTimeOffset ConvertLocalToUtc(DateOnly date, TimeOnly time)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Unspecified);
        while (ApplicationTimeZone.IsInvalidTime(local))
        {
            local = local.AddMinutes(1);
        }

        var utc = TimeZoneInfo.ConvertTimeToUtc(local, ApplicationTimeZone);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }

    public DateOnly GetLocalDate(DateTimeOffset utc)
        => DateOnly.FromDateTime(ConvertUtcToLocal(utc).DateTime);

    public bool IsWithinStartWindow(DateTimeOffset nowUtc, DateTimeOffset scheduledStartUtc, int earlyMinutes, int lateMinutes)
        => nowUtc >= scheduledStartUtc.AddMinutes(-earlyMinutes) &&
           nowUtc <= scheduledStartUtc.AddMinutes(lateMinutes);

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId, string windowsTimeZoneId)
    {
        foreach (var candidate in new[] { timeZoneId, windowsTimeZoneId, "UTC" }.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(candidate);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }
}
