using Application.Abstractions.Repositories;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.GetById;

public class GetIngredientByIdQueryHandler : IRequestHandler<GetIngredientByIdQuery, IngredientResponse?>
{
    private readonly IIngredientsRepository _repo;

    public GetIngredientByIdQueryHandler(IIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IngredientResponse?> Handle(GetIngredientByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        return entity?.Adapt<IngredientResponse>();
    }
}
