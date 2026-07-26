using AttendAI.Application.Attendance;
using AttendAI.Domain.Enums;
using AttendAI.Infrastructure.Attendance;

namespace AttendAI.UnitTests.Application;

public sealed class LocationVerificationServiceTests
{
    private readonly LocationVerificationService _service = new();

    [Fact]
    public void Verify_accepts_same_point_with_accurate_browser_location()
    {
        var result = _service.Verify(
            new BrowserLocationSample(31.9539, 35.9106, 10, DateTimeOffset.UtcNow),
            Policy());

        Assert.True(result.Accepted);
        Assert.Equal(LocationVerificationOutcome.Accepted, result.Outcome);
        Assert.Equal(0m, result.DistanceMeters);
    }

    [Fact]
    public void Verify_rejects_outside_geofence_without_expanding_radius_by_accuracy()
    {
        var result = _service.Verify(
            new BrowserLocationSample(31.9550, 35.9106, 5, DateTimeOffset.UtcNow),
            Policy(allowedRadiusMeters: 50));

        Assert.False(result.Accepted);
        Assert.Equal(LocationVerificationOutcome.OutsideGeofence, result.Outcome);
        Assert.Equal(AttendanceFailureReason.OutsideGeofence, result.FailureReason);
    }

    [Fact]
    public void Verify_rejects_browser_accuracy_above_limit()
    {
        var result = _service.Verify(
            new BrowserLocationSample(31.9539, 35.9106, 76, DateTimeOffset.UtcNow),
            Policy(maximumAcceptedAccuracyMeters: 75));

        Assert.False(result.Accepted);
        Assert.Equal(LocationVerificationOutcome.Inaccurate, result.Outcome);
    }

    [Theory]
    [InlineData(double.NaN, 35.9106, 5)]
    [InlineData(91, 35.9106, 5)]
    [InlineData(31.9539, 181, 5)]
    [InlineData(31.9539, 35.9106, -1)]
    public void Verify_rejects_invalid_coordinates_or_accuracy(double latitude, double longitude, double accuracy)
    {
        var result = _service.Verify(
            new BrowserLocationSample(latitude, longitude, accuracy, DateTimeOffset.UtcNow),
            Policy());

        Assert.False(result.Accepted);
        Assert.Equal(LocationVerificationOutcome.Invalid, result.Outcome);
    }

    [Fact]
    public void Verify_rejects_missing_server_policy()
    {
        var result = _service.Verify(
            new BrowserLocationSample(31.9539, 35.9106, 10, DateTimeOffset.UtcNow),
            new AttendanceLocationPolicy(null, null, 50, 75, LocationRequired: true));

        Assert.False(result.Accepted);
        Assert.Equal(LocationVerificationOutcome.MissingPolicy, result.Outcome);
    }

    private static AttendanceLocationPolicy Policy(int allowedRadiusMeters = 50, int maximumAcceptedAccuracyMeters = 75)
        => new(31.9539m, 35.9106m, allowedRadiusMeters, maximumAcceptedAccuracyMeters, LocationRequired: true);
}
