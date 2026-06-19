using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItem.Delete;

public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, OperationResponse>
{
    private readonly IMenuItemsRepository _repo;
    private readonly ICloudinaryService _cloudinaryService;

    public DeleteMenuItemCommandHandler(
        IMenuItemsRepository repo,
        ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<OperationResponse> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Menu item not found");

        var picturePublicIds = existing.MenuItemPictures
            .Select(picture => picture.ImagePublicId)
            .Where(publicId => !string.IsNullOrWhiteSpace(publicId))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        await _repo.DeleteAsync(request.Id, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        var allPicturesDeleted = true;
        foreach (var publicId in picturePublicIds)
        {
            try
            {
                allPicturesDeleted &= await _cloudinaryService.DeleteImageAsync(
                    publicId,
                    cancellationToken);
            }
            catch
            {
                allPicturesDeleted = false;
            }
        }

        return new OperationResponse(
            true,
            allPicturesDeleted
                ? "Menu item deleted."
                : "Menu item deleted, but some picture cleanup could not be confirmed.");
    }
}
