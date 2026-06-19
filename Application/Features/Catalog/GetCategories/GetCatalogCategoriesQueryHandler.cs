using Application.Abstractions.Repositories;
using Application.DTOs.Catalog;
using Application.DTOs.Common;
using Mapster;
using MediatR;

namespace Application.Features.Catalog.GetCategories;

public sealed class GetCatalogCategoriesQueryHandler
    : IRequestHandler<GetCatalogCategoriesQuery, PaginatedResponse<CatalogCategoryResponse>>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public GetCatalogCategoriesQueryHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public async Task<PaginatedResponse<CatalogCategoryResponse>> Handle(
        GetCatalogCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _categoriesRepository.CountAsync(cancellationToken);
        var categories = await _categoriesRepository.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            cancellationToken);

        var items = categories.Adapt<List<CatalogCategoryResponse>>();

        return PaginatedResponse<CatalogCategoryResponse>.Create(pagination, totalCount, items);
    }
}
