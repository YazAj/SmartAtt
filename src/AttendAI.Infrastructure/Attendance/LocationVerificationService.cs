using AttendAI.Application.Attendance;
using AttendAI.Domain.Enums;

namespace AttendAI.Infrastructure.Attendance;

public sealed class LocationVerificationService : ILocationVerificationService
{
    private const double EarthRadiusMeters = 6371000d;

    public LocationVerificationResult Verify(BrowserLocationSample sample, AttendanceLocationPolicy policy)
    {
        if (!policy.LocationRequired)
        {
            return new LocationVerificationResult(LocationVerificationOutcome.Accepted, AttendanceFailureReason.None, null, "AttendanceLocationAccepted");
        }

        if (!policy.Latitude.HasValue || !policy.Longitude.HasValue)
        {
            return new LocationVerificationResult(LocationVerificationOutcome.MissingPolicy, AttendanceFailureReason.AttendanceLocationUnavailable, null, "ErrorAttendanceLocationPolicyMissing");
        }

        if (!IsFinite(sample.Latitude) ||
            !IsFinite(sample.Longitude) ||
            !IsFinite(sample.AccuracyMeters) ||
            sample.Latitude is < -90d or > 90d ||
            sample.Longitude is < -180d or > 180d ||
            sample.AccuracyMeters < 0)
        {
            return new LocationVerificationResult(LocationVerificationOutcome.Invalid, AttendanceFailureReason.LocationInvalid, null, "ErrorAttendanceLocationInvalid");
        }

        if (sample.AccuracyMeters > policy.MaximumAcceptedAccuracyMeters)
        {
            return new LocationVerificationResult(
                LocationVerificationOutcome.Inaccurate,
                AttendanceFailureReason.LocationInaccurate,
                null,
                "ErrorAttendanceLocationInaccurate");
        }

        var distanceMeters = HaversineMeters(
            sample.Latitude,
            sample.Longitude,
            (double)policy.Latitude.Value,
            (double)policy.Longitude.Value);
        var roundedDistance = DecimalRound(distanceMeters);

        return distanceMeters <= policy.AllowedRadiusMeters
            ? new LocationVerificationResult(LocationVerificationOutcome.Accepted, AttendanceFailureReason.None, roundedDistance, "AttendanceLocationAccepted")
            : new LocationVerificationResult(LocationVerificationOutcome.OutsideGeofence, AttendanceFailureReason.OutsideGeofence, roundedDistance, "ErrorAttendanceOutsideGeofence");
    }

    private static double HaversineMeters(double latitude, double longitude, double targetLatitude, double targetLongitude)
    {
        var latitudeRadians = DegreesToRadians(latitude);
        var targetLatitudeRadians = DegreesToRadians(targetLatitude);
        var deltaLatitude = DegreesToRadians(targetLatitude - latitude);
        var deltaLongitude = DegreesToRadians(targetLongitude - longitude);

        var a = Math.Pow(Math.Sin(deltaLatitude / 2d), 2d) +
            Math.Cos(latitudeRadians) *
            Math.Cos(targetLatitudeRadians) *
            Math.Pow(Math.Sin(deltaLongitude / 2d), 2d);
        var clamped = Math.Min(1d, Math.Max(0d, a));
        return EarthRadiusMeters * 2d * Math.Atan2(Math.Sqrt(clamped), Math.Sqrt(1d - clamped));
    }

    private static double DegreesToRadians(double degrees)
        => degrees * Math.PI / 180d;

    private static bool IsFinite(double value)
        => !double.IsNaN(value) && !double.IsInfinity(value);

    private static decimal DecimalRound(double value)
        => Math.Round((decimal)value, 3, MidpointRounding.AwayFromZero);
}
