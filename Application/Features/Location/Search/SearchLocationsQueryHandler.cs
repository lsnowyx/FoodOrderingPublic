using Application.Abstractions.Services;
using Application.DTOs.Location;
using MediatR;

namespace Application.Features.Location.Search;

public sealed class SearchLocationsQueryHandler
    : IRequestHandler<SearchLocationsQuery, IReadOnlyList<LocationSearchResult>>
{
    private const int MinimumQueryLength = 3;
    private const int MinimumLimit = 1;
    private const int MaximumLimit = 10;

    private readonly IGeocodingService _geocodingService;

    public SearchLocationsQueryHandler(IGeocodingService geocodingService)
    {
        _geocodingService = geocodingService;
    }

    public async Task<IReadOnlyList<LocationSearchResult>> Handle(
        SearchLocationsQuery request,
        CancellationToken cancellationToken)
    {
        var query = request.Query.Trim();
        if (query.Length < MinimumQueryLength)
        {
            throw new ArgumentException(
                $"Location search query must contain at least {MinimumQueryLength} characters.",
                nameof(request.Query));
        }

        var limit = request.Limit.HasValue
            ? Math.Clamp(request.Limit.Value, MinimumLimit, MaximumLimit)
            : request.Limit;

        return await _geocodingService.SearchAsync(
            query,
            limit,
            cancellationToken);
    }
}
