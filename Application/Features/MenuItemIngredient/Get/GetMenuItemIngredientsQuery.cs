using Application.DTOs.MenuItemIngredient;
using MediatR;

namespace Application.Features.MenuItemIngredient.Get;

public sealed class GetMenuItemIngredientsQuery : IRequest<IEnumerable<MenuItemIngredientResponse>>
{
    public Guid MenuItemId { get; set; }
}
