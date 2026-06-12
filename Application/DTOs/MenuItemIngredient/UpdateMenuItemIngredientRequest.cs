using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItemIngredient;

public sealed record UpdateMenuItemIngredientRequest(
    [param: Required]
    [param: StringLength(50)]
    string Quantity
);
