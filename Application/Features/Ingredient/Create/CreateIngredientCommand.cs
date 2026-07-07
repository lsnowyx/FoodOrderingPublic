using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.Create;

using System.ComponentModel.DataAnnotations;

public sealed class CreateIngredientCommand : IRequest<IngredientResponse>
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string BaseUnit { get; set; } = null!;

    [Range(typeof(decimal), "0", "99999999999999.9999", ParseLimitsInInvariantCulture = true)]
    public decimal UnitCost { get; set; }

    [StringLength(500)]
    public string? AllergenInfo { get; set; }

    [Range(0, 10000)]
    public int? CaloriesPerUnit { get; set; }
}
