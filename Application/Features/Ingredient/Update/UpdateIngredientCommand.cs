using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.Update;
public sealed class UpdateIngredientCommand : IRequest<IngredientResponse>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? AllergenInfo { get; set; }

    public int? CaloriesPerUnit { get; set; }
}
