using Application.Abstractions.Repositories;
using Application.DTOs.MenuItemIngredient;
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
        return list.Select(mi => new MenuItemIngredientResponse(mi.Id, mi.IngredientId, mi.Ingredient?.Name ?? string.Empty, mi.Ingredient?.AllergenInfo ?? string.Empty, mi.Ingredient?.CaloriesPerUnit ?? 0, mi.Quantity ?? string.Empty));
    }
}
