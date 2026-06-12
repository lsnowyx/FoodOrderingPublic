using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItemIngredient;

public sealed record CreateMenuItemIngredientRequest(
    [param: Required]
    Guid IngredientId,

    [param: Required]
    [param: StringLength(50)]
    string Quantity
);
