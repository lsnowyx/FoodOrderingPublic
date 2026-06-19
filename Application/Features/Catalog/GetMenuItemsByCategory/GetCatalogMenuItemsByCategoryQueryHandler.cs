using Application.Abstractions.Repositories;
using Application.DTOs.Catalog;
using Application.DTOs.Common;
using Mapster;
using MediatR;

namespace Application.Features.Catalog.GetMenuItemsByCategory;

public sealed class GetCatalogMenuItemsByCategoryQueryHandler
    : IRequestHandler<GetCatalogMenuItemsByCategoryQuery, PaginatedResponse<CatalogMenuItemResponse>>
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IMenuItemsRepository _menuItemsRepository;

    public GetCatalogMenuItemsByCategoryQueryHandler(
        ICategoriesRepository categoriesRepository,
        IMenuItemsRepository menuItemsRepository)
    {
        _categoriesRepository = categoriesRepository;
        _menuItemsRepository = menuItemsRepository;
    }

    public async Task<PaginatedResponse<CatalogMenuItemResponse>> Handle(
        GetCatalogMenuItemsByCategoryQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoriesRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _menuItemsRepository.CountAvailableByCategoryAsync(request.CategoryId, cancellationToken);
        var menuItems = await _menuItemsRepository.GetAvailableByCategoryPagedAsync(
            request.CategoryId,
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = menuItems.Adapt<List<CatalogMenuItemResponse>>();

        return PaginatedResponse<CatalogMenuItemResponse>.Create(pagination, totalCount, items);
    }
}
