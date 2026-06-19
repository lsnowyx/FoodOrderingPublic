using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItem;

public sealed record CreateMenuItemRequest(
    [param: Required]
    [param: StringLength(100)]
    string Name,

    [param: StringLength(1000)]
    string? Description,

    [param: Range(0, 1_000_000)]
    decimal Price,

    Guid CategoryId,

    bool IsAvailable = true
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CategoryId == Guid.Empty)
        {
            yield return new ValidationResult(
                "Category is required.",
                new[] { nameof(CategoryId) });
        }
    }
}
