using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.MenuItemPicture;
using MediatR;

namespace Application.Features.MenuItemPicture.Create;

public class CreateMenuItemPictureCommandHandler : IRequestHandler<CreateMenuItemPictureCommand, MenuItemPictureResponse>
{
    private readonly IMenuItemPicturesRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly ICloudinaryService cloudinaryService;

    public CreateMenuItemPictureCommandHandler(IMenuItemPicturesRepository repo, IMenuItemsRepository menuRepo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        this.cloudinaryService = cloudinaryService;
    }

    public async Task<MenuItemPictureResponse> Handle(CreateMenuItemPictureCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menuRepo.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("Menu item not found");

        var cloudinaryResponse = await cloudinaryService.AddImageAsync(request.ImageFile, cancellationToken);


        var toAdd = new Domain.Entities.MenuItemPicture
        {
            MenuItemId = request.MenuItemId,
            ImageUrl = cloudinaryResponse.ImageUrl,
            ImagePublicId = cloudinaryResponse.PublicId,
            Caption = request.Caption
        };

        await _repo.AddAsync(toAdd, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return new MenuItemPictureResponse(toAdd.Id, toAdd.MenuItemId, toAdd.ImageUrl, toAdd.Caption);
    }
}
