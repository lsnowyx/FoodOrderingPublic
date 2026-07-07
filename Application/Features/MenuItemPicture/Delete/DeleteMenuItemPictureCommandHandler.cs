using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItemPicture.Delete;

public class DeleteMenuItemPictureCommandHandler : IRequestHandler<DeleteMenuItemPictureCommand, OperationResponse>
{
    private readonly IMenuItemPicturesRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly ICloudinaryService cloudinaryService;
    private readonly ICacheService cacheService;

    public DeleteMenuItemPictureCommandHandler(
        IMenuItemPicturesRepository repo,
        IMenuItemsRepository menuRepo,
        ICloudinaryService cloudinaryService,
        ICacheService cacheService)
    {
        _repo = repo;
        _menuRepo = menuRepo;
        this.cloudinaryService = cloudinaryService;
        this.cacheService = cacheService;
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
        await CacheInvalidationHelper.InvalidateMenuItemCachesAsync(
            cacheService,
            cancellationToken);

        var imageDeleted = true;
        if (!string.IsNullOrWhiteSpace(publicId))
        {
            try
            {
                imageDeleted = await cloudinaryService.DeleteImageAsync(publicId, cancellationToken);
            }
            catch
            {
                imageDeleted = false;
            }
        }

        return new OperationResponse(
            true,
            imageDeleted
                ? "Menu item picture deleted."
                : "Menu item picture deleted, but image cleanup could not be confirmed.");
    }
}
