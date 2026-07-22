namespace AttendAI.Domain.Academic;

public sealed class Classroom : AcademicEntity
{
    private Classroom()
    {
        Code = string.Empty;
        BuildingNameEnglish = string.Empty;
        BuildingNameArabic = string.Empty;
        RoomNumber = string.Empty;
    }

    public Classroom(
        string code,
        string buildingNameEnglish,
        string buildingNameArabic,
        string roomNumber,
        int capacity,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        Update(code, buildingNameEnglish, buildingNameArabic, roomNumber, capacity, latitude, longitude);
    }

    public string Code { get; private set; } = string.Empty;

    public string BuildingNameEnglish { get; private set; } = string.Empty;

    public string BuildingNameArabic { get; private set; } = string.Empty;

    public string RoomNumber { get; private set; } = string.Empty;

    public int Capacity { get; private set; }

    public decimal? Latitude { get; private set; }

    public decimal? Longitude { get; private set; }

    public void Update(
        string code,
        string buildingNameEnglish,
        string buildingNameArabic,
        string roomNumber,
        int capacity,
        decimal? latitude,
        decimal? longitude)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
        }

        if (latitude.HasValue != longitude.HasValue)
        {
            throw new ArgumentException("Latitude and longitude must be provided together.");
        }

        if (latitude is < -90m or > 90m)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }

        if (longitude is < -180m or > 180m)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }

        Code = NormalizeCode(code, nameof(Code), 32);
        BuildingNameEnglish = RequireText(buildingNameEnglish, nameof(BuildingNameEnglish), 160);
        BuildingNameArabic = RequireText(buildingNameArabic, nameof(BuildingNameArabic), 160);
        RoomNumber = RequireText(roomNumber, nameof(RoomNumber), 60).ToUpperInvariant();
        Capacity = capacity;
        Latitude = latitude;
        Longitude = longitude;
        Touch();
    }
}
