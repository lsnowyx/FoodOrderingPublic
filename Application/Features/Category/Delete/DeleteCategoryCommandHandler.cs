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
            return new OperationResponse(true, "Category deleted. Without Cloudinary Deletion");
        }

        await _repo.DeleteAsync(request.Id, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return new OperationResponse(true, "Category deleted. With Cloudinary Deletion");
    }
}
