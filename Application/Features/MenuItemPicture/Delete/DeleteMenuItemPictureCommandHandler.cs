using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItemPicture.Delete;

public class DeleteMenuItemPictureCommandHandler : IRequestHandler<DeleteMenuItemPictureCommand, OperationResponse>
{
    private readonly IMenuItemPicturesRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly ICloudinaryService cloudinaryService;

    public DeleteMenuItemPictureCommandHandler(IMenuItemPicturesRepository repo, IMenuItemsRepository menuRepo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        this.cloudinaryService = cloudinaryService;
    }

    public async Task<OperationResponse> Handle(DeleteMenuItemPictureCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menuRepo.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("Menu item not found");

        var pic = await _repo.GetByIdAsync(request.PictureId, cancellationToken);
        if (pic == null || pic.MenuItemId != request.MenuItemId) throw new KeyNotFoundException("Picture not found for this menu item");

        var publicId = pic.ImagePublicId;

        await _repo.DeleteAsync(pic, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        try
        {
            if (!string.IsNullOrWhiteSpace(publicId))
            {
                await cloudinaryService.DeleteImageAsync(publicId, cancellationToken);
            }
        }
        catch
        {
            // Log this later.
            // Do not fail the whole operation because the DB delete already succeeded.
            //Schedule deletion later.
            return new OperationResponse(true, "Menu item picture deleted. Without Cloudinary Deletion");
        }

        return new OperationResponse(true, "Menu item picture deleted. With Cloudinary Deletion");
    }
}
