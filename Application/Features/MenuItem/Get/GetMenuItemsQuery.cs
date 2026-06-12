using Application.DTOs.MenuItem;
using MediatR;

namespace Application.Features.MenuItem.Get;

public sealed class GetMenuItemsQuery : IRequest<IEnumerable<MenuItemResponse>> { }
