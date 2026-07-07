using Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public sealed class AdjustItemDto : IValidatableObject
{
    public Guid ItemId { get; set; }

    [Range(1, OrderValidationConstants.MaxItemQuantity)]
    public int Quantity { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ItemId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Order item id is required.",
                new[] { nameof(ItemId) });
        }
    }
}
