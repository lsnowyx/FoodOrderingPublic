using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models.Ingredient;

public class IngredientViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? AllergenInfo { get; set; }

    [Range(0, 10000)]
    public int? CaloriesPerUnit { get; set; }
}
