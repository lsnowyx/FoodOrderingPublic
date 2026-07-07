using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Ingredient;

public sealed record CreateIngredientRequest(
    [param: Required]
    [param: StringLength(100)]
    string Name,

    [param: Required]
    [param: StringLength(50)]
    string BaseUnit,

    [param: Required]
    [param: Range(typeof(decimal), "0", "99999999999999.9999", ParseLimitsInInvariantCulture = true)]
    decimal? UnitCost,

    [param: StringLength(500)]
    string? AllergenInfo,

    [param: Range(0, 10000)] int? CaloriesPerUnit);
