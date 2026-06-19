using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Category.Delete;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, OperationResponse>
{
    private readonly ICategoriesRepository _repo;
    private readonly ICloudinaryService cloudinaryService;

    public DeleteCategoryCommandHandler(ICategoriesRepository repo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        this.cloudinaryService = cloudinaryService;
    }

    public async Task<OperationResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Category not found");

        var publicId = existing.ImagePublicId;

        await _repo.DeleteAsync(request.Id, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

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
                ? "Category deleted."
                : "Category deleted, but image cleanup could not be confirmed.");
    }
}
