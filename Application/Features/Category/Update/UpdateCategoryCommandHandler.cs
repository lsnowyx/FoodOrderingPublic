using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Category;
using Mapster;
using MediatR;

namespace Application.Features.Category.Update;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    private readonly ICategoriesRepository _repo;
    private readonly ICloudinaryService _cloudinaryService;

    public UpdateCategoryCommandHandler(ICategoriesRepository repo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Category not found");

        existing.Name = request.Name;
        existing.Description = request.Description;

        if (request.ImageFile is not null)
        {
            var oldPublicId = existing.ImagePublicId;

            var cloudinaryResponse = await _cloudinaryService.AddImageAsync(request.ImageFile, cancellationToken);
            existing.ImageUrl = cloudinaryResponse.ImageUrl;
            existing.ImagePublicId = cloudinaryResponse.PublicId;

            try
            {
                await _repo.UpdateAsync(existing, cancellationToken);
                await _repo.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                try
                {
                    await _cloudinaryService.DeleteImageAsync(
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
                    await _cloudinaryService.DeleteImageAsync(oldPublicId, cancellationToken);
                }
                catch
                {
                    // The category already points to the new image.
                }
            }

            return existing.Adapt<CategoryResponse>();
        }

        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return existing.Adapt<CategoryResponse>();
    }
}
