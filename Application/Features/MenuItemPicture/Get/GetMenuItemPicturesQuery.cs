using Application.DTOs.MenuItemPicture;
using MediatR;

namespace Application.Features.MenuItemPicture.Get;

public sealed class GetMenuItemPicturesQuery : IRequest<IEnumerable<MenuItemPictureResponse>>
{
    public Guid MenuItemId { get; set; }
}
