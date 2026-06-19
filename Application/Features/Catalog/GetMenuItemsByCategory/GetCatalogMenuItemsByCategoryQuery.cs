using Application.DTOs.Catalog;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Catalog.GetMenuItemsByCategory;

public sealed class GetCatalogMenuItemsByCategoryQuery : IRequest<PaginatedResponse<CatalogMenuItemResponse>>
{
    public Guid CategoryId { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
}
