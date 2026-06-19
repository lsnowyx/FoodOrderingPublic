namespace AdminPanel.Models.Lookup;

public class IngredientLookupViewModel
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string BaseUnit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public string? AllergenInfo { get; set; }
    public int? CaloriesPerUnit { get; set; }
}
