using Application.DTOs.Catalog;
using MediatR;

namespace Application.Features.Catalog.GetMenuItemDetails;

public sealed class GetCatalogMenuItemDetailsQuery : IRequest<CatalogMenuItemDetailsResponse>
{
    public Guid MenuItemId { get; init; }
}
