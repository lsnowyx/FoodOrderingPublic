namespace Application.DTOs.Location;

public sealed record LocationSearchResult(
    string DisplayName,
    double Latitude,
    double Longitude,
    string? Type,
    double? Importance);
