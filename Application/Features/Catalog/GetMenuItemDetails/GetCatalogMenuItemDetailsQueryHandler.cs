using Application.Abstractions.Repositories;
using Application.DTOs.Catalog;
using Mapster;
using MediatR;

namespace Application.Features.Catalog.GetMenuItemDetails;

public sealed class GetCatalogMenuItemDetailsQueryHandler
    : IRequestHandler<GetCatalogMenuItemDetailsQuery, CatalogMenuItemDetailsResponse>
{
    private readonly IMenuItemsRepository _menuItemsRepository;

    public GetCatalogMenuItemDetailsQueryHandler(IMenuItemsRepository menuItemsRepository)
    {
        _menuItemsRepository = menuItemsRepository;
    }

    public async Task<CatalogMenuItemDetailsResponse> Handle(
        GetCatalogMenuItemDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var menuItem = await _menuItemsRepository.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menuItem == null || !menuItem.IsAvailable)
        {
            throw new KeyNotFoundException("Menu item not found");
        }

        return menuItem.Adapt<CatalogMenuItemDetailsResponse>();
    }
}
