using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Ingredient;

public sealed record CreateIngredientRequest(
    [param: Required][param: StringLength(100)] string Name, 
    [param: StringLength(500)] string? AllergenInfo, 
    [param: Range(0, 10000)] int? CaloriesPerUnit);
