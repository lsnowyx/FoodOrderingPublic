namespace AdminPanel.Models.Lookup;

public class Select2LookupItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BaseUnit { get; set; }
    public decimal? UnitCost { get; set; }
    public string? AllergenInfo { get; set; }
    public int? CaloriesPerUnit { get; set; }
}
