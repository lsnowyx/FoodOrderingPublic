using AdminPanel.Models.Common;

namespace AdminPanel.Models.Ingredient;

public class IngredientIndexViewModel
{
    public string? SearchTerm { get; set; }
    public PaginatedViewModel<IngredientViewModel> Ingredients { get; set; } = new();
}
