using Application.DTOs.MenuItem;
using MediatR;

namespace Application.Features.MenuItem.GetById;

public sealed class GetMenuItemByIdQuery : IRequest<MenuItemDetailsResponse?>
{
    public Guid Id { get; set; }
}
