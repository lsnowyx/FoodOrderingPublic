using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.MenuItemPicture;
using Mapster;
using MediatR;

namespace Application.Features.MenuItemPicture.Create;

public class CreateMenuItemPictureCommandHandler : IRequestHandler<CreateMenuItemPictureCommand, MenuItemPictureResponse>
{
    private readonly IMenuItemPicturesRepository _repo;
    private readonly IMenuItemsRepository _menuRepo;
    private readonly ICloudinaryService cloudinaryService;
    private readonly ICacheService cacheService;

    public CreateMenuItemPictureCommandHandler(
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

        try
        {
            await _repo.AddAsync(toAdd, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            await CacheInvalidationHelper.InvalidateMenuItemCachesAsync(
                cacheService,
                cancellationToken);
        }
        catch
        {
            try
            {
                await cloudinaryService.DeleteImageAsync(cloudinaryResponse.PublicId, cancellationToken);
            }
            catch
            {
                // The original persistence failure is more important than cleanup failure.
            }

            throw;
        }

        return toAdd.Adapt<MenuItemPictureResponse>();
    }
}
