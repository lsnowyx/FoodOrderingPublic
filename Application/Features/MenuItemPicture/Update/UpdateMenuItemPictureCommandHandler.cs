using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.MenuItemPicture;
using Mapster;
using MediatR;

namespace Application.Features.MenuItemPicture.Update;

public class UpdateMenuItemPictureCommandHandler : IRequestHandler<UpdateMenuItemPictureCommand, MenuItemPictureResponse>
{
    private readonly IMenuItemPicturesRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly ICloudinaryService cloudinaryService;

    public UpdateMenuItemPictureCommandHandler(IMenuItemPicturesRepository repo, IMenuItemsRepository menuRepo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        this.cloudinaryService = cloudinaryService;
    }

    public async Task<MenuItemPictureResponse> Handle(UpdateMenuItemPictureCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menuRepo.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("Menu item not found");

        var pic = await _repo.GetByIdAsync(request.PictureId, cancellationToken);
        if (pic == null || pic.MenuItemId != request.MenuItemId) throw new KeyNotFoundException("Picture not found for this menu item");


        var oldPublicId = pic.ImagePublicId;

        var cloudinaryResponse = await cloudinaryService.AddImageAsync(request.ImageFile, cancellationToken);

        pic.ImageUrl = cloudinaryResponse.ImageUrl;
        pic.ImagePublicId = cloudinaryResponse.PublicId;
        pic.Caption = request.Caption;

        try
        {
            await _repo.UpdateAsync(pic, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await cloudinaryService.DeleteImageAsync(
                    cloudinaryResponse.PublicId,
                    cancellationToken);
            }
            catch
            {
                // Preserve the original persistence exception.
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldPublicId))
        {
            try
            {
                await cloudinaryService.DeleteImageAsync(oldPublicId, cancellationToken);
            }
            catch
            {
                // The picture already points to the new image.
            }
        }

        return pic.Adapt<MenuItemPictureResponse>();
    }
}
