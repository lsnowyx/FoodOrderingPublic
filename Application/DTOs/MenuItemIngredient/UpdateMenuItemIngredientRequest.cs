using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItemIngredient;

public sealed record UpdateMenuItemIngredientRequest(
    [param: Required]
    [param: Range(typeof(decimal), "0.0001", "99999999999999.9999")]
    decimal? Quantity
);
