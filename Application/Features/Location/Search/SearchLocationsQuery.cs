using Application.DTOs.Location;
using MediatR;

namespace Application.Features.Location.Search;

public sealed class SearchLocationsQuery : IRequest<IReadOnlyList<LocationSearchResult>>
{
    public string Query { get; init; } = string.Empty;

    public int? Limit { get; init; }
}
