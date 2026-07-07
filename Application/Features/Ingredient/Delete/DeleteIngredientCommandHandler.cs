using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Ingredient.Delete;

public class DeleteIngredientCommandHandler : IRequestHandler<DeleteIngredientCommand, OperationResponse>
{
    private readonly IIngredientsRepository _repo;
    private readonly ICacheService _cacheService;

    public DeleteIngredientCommandHandler(
        IIngredientsRepository repo,
        ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<OperationResponse> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Ingredient not found");

        var used = await _repo.IsUsedAsync(request.Id, cancellationToken);
        if (used) throw new InvalidOperationException("Ingredient is used by menu items and cannot be deleted.");

        await _repo.DeleteAsync(request.Id, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        await CacheInvalidationHelper.InvalidateIngredientCachesAsync(
            _cacheService,
            cancellationToken);
        return new OperationResponse(true, "Ingredient deleted.");
    }
}
