using Application.Abstractions.Repositories;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Get;

public class GetIngredientsQueryHandler : IRequestHandler<GetIngredientsQuery, IEnumerable<IngredientResponse>>
{
    private readonly IIngredientsRepository _repo;

    public GetIngredientsQueryHandler(IIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<IngredientResponse>> Handle(GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        var all = await _repo.GetAllAsync(cancellationToken);
        return all.Select(i => i.Adapt<IngredientResponse>());
    }
}
