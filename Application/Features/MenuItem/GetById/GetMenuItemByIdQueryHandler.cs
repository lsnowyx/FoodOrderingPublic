using Application.Abstractions.Repositories;
using Application.DTOs.MenuItem;
using Mapster;
using MediatR;

namespace Application.Features.MenuItem.GetById;

public class GetMenuItemByIdQueryHandler : IRequestHandler<GetMenuItemByIdQuery, MenuItemDetailsResponse?>
{
    private readonly IMenuItemsRepository _repo;

    public GetMenuItemByIdQueryHandler(IMenuItemsRepository repo)
    {
        _repo = repo;
    }

    public async Task<MenuItemDetailsResponse?> Handle(GetMenuItemByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return null;

        return entity.Adapt<MenuItemDetailsResponse>();
    }
}
