using Application.Abstractions.Repositories;
using Application.DTOs.MenuItem;
using Mapster;
using MediatR;

namespace Application.Features.MenuItem.Get;

public class GetMenuItemsQueryHandler : IRequestHandler<GetMenuItemsQuery, IEnumerable<MenuItemResponse>>
{
    private readonly IMenuItemsRepository _repo;

    public GetMenuItemsQueryHandler(IMenuItemsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<MenuItemResponse>> Handle(GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var all = await _repo.GetAllAsync(cancellationToken);
        return all.Select(m => m.Adapt<MenuItemResponse>());
    }
}
