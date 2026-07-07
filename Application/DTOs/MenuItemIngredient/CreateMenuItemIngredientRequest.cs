using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItemIngredient;

public sealed record CreateMenuItemIngredientRequest(
    Guid IngredientId,

    [param: Required]
    [param: Range(typeof(decimal), "0.0001", "99999999999999.9999", ParseLimitsInInvariantCulture = true)]
    decimal? Quantity
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IngredientId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Ingredient id is required.",
                new[] { nameof(IngredientId) });
        }
    }
}
