using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Create;

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, IngredientResponse>
{
    private readonly IIngredientsRepository _repo;
    private readonly ICacheService _cacheService;

    public CreateIngredientCommandHandler(
        IIngredientsRepository repo,
        ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<IngredientResponse> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var toAdd = request.Adapt<Domain.Entities.Ingredient>();
        toAdd.BaseUnit = request.BaseUnit.Trim();
        await _repo.AddAsync(toAdd, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        await CacheInvalidationHelper.InvalidateIngredientCachesAsync(
            _cacheService,
            cancellationToken);
        return toAdd.Adapt<IngredientResponse>();
    }
}
