using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models.Ingredient;

public class IngredientViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Base Unit")]
    public string BaseUnit { get; set; } = string.Empty;

    [Required]
    [Range(typeof(decimal), "0", "99999999999999.9999")]
    [Display(Name = "Cost per Base Unit")]
    public decimal? UnitCost { get; set; }

    [StringLength(500)]
    public string? AllergenInfo { get; set; }

    [Range(0, 10000)]
    public int? CaloriesPerUnit { get; set; }
}
