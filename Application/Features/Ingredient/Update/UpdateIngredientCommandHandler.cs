using Application.Abstractions.Repositories;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Update;

public class UpdateIngredientCommandHandler : IRequestHandler<UpdateIngredientCommand, IngredientResponse>
{
    private readonly IIngredientsRepository _repo;

    public UpdateIngredientCommandHandler(IIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IngredientResponse> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Ingredient not found");

        existing.Name = request.Name;
        existing.AllergenInfo = request.AllergenInfo;
        existing.CaloriesPerUnit = request.CaloriesPerUnit;

        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return existing.Adapt<IngredientResponse>();
    }
}
