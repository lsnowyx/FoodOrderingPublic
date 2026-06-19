using Application.DTOs.Catalog;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Catalog.GetCategories;

public sealed class GetCatalogCategoriesQuery : IRequest<PaginatedResponse<CatalogCategoryResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
}
