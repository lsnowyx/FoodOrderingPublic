using Application.Abstractions.Repositories;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Create;

public class CreateIngredientCommandHandler : IRequestHandler<CreateIngredientCommand, IngredientResponse>
{
    private readonly IIngredientsRepository _repo;

    public CreateIngredientCommandHandler(IIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IngredientResponse> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var toAdd = request.Adapt<Domain.Entities.Ingredient>();
        await _repo.AddAsync(toAdd, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return toAdd.Adapt<IngredientResponse>();
    }
}
