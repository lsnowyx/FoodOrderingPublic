using Application.Abstractions.Repositories;
using Application.DTOs.MenuItemIngredient;
using Mapster;
using MediatR;

namespace Application.Features.MenuItemIngredient.Get;

public class GetMenuItemIngredientsQueryHandler : IRequestHandler<GetMenuItemIngredientsQuery, IEnumerable<MenuItemIngredientResponse>>
{
    private readonly IMenuItemIngredientsRepository _repo;

    public GetMenuItemIngredientsQueryHandler(IMenuItemIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<MenuItemIngredientResponse>> Handle(GetMenuItemIngredientsQuery request, CancellationToken cancellationToken)
    {
        var list = await _repo.GetByMenuItemIdAsync(request.MenuItemId, cancellationToken);
        return list.Adapt<List<MenuItemIngredientResponse>>();
    }
}
