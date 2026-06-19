using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Category;
using Mapster;
using MediatR;

namespace Application.Features.Category.Create;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICategoriesRepository _repo;
    private readonly ICloudinaryService _cloudinaryService;

    public CreateCategoryCommandHandler(ICategoriesRepository repo, ICloudinaryService cloudinaryService)
    {
        _repo = repo;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var toAdd = request.Adapt<Domain.Entities.Category>();
        string? uploadedPublicId = null;

        if (request.ImageFile is not null)
        {
            var cloudinaryResponse = await _cloudinaryService.AddImageAsync(request.ImageFile, cancellationToken);
            toAdd.ImageUrl = cloudinaryResponse.ImageUrl;
            toAdd.ImagePublicId = cloudinaryResponse.PublicId;
            uploadedPublicId = cloudinaryResponse.PublicId;
        }

        try
        {
            await _repo.AddAsync(toAdd, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(uploadedPublicId))
            {
                try
                {
                    await _cloudinaryService.DeleteImageAsync(uploadedPublicId, cancellationToken);
                }
                catch
                {
                    // The original persistence failure is more important than cleanup failure.
                }
            }

            throw;
        }

        return toAdd.Adapt<CategoryResponse>();
    }
}
