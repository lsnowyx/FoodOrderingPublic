using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    public string? AllergenInfo { get; set; }

    public int? CaloriesPerUnit { get; set; }

    public List<MenuItemIngredient> MenuItemIngredients { get; set; } = new();
}