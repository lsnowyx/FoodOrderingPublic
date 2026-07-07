using Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public class CreateOrderItemDto : IValidatableObject
{
    public Guid MenuItemId { get; set; }

    [Range(1, OrderValidationConstants.MaxItemQuantity)]
    public int Quantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MenuItemId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Menu item id is required.",
                new[] { nameof(MenuItemId) });
        }
    }
}
