using Application.DTOs.Location;

namespace Application.Abstractions.Services;

public interface IGeocodingService
{
    Task<IReadOnlyList<LocationSearchResult>> SearchAsync(
        string query,
        int? limit,
        CancellationToken cancellationToken = default);
}
