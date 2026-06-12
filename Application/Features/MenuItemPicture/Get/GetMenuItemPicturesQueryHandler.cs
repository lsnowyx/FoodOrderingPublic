using Application.Abstractions.Repositories;
using Application.DTOs.MenuItemPicture;
using MediatR;

namespace Application.Features.MenuItemPicture.Get;

public class GetMenuItemPicturesQueryHandler : IRequestHandler<GetMenuItemPicturesQuery, IEnumerable<MenuItemPictureResponse>>
{
    private readonly IMenuItemPicturesRepository _repo;

    public GetMenuItemPicturesQueryHandler(IMenuItemPicturesRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<MenuItemPictureResponse>> Handle(GetMenuItemPicturesQuery request, CancellationToken cancellationToken)
    {
        var list = await _repo.GetByMenuItemIdAsync(request.MenuItemId, cancellationToken);
        return list.Select(p => new MenuItemPictureResponse(p.Id, p.MenuItemId, p.ImageUrl, p.Caption));
    }
}
