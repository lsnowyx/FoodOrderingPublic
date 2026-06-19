using System;
using System.Collections.Generic;

namespace AdminPanel.Models.MenuItem;

public class MenuItemIngredientViewModel
{
    public Guid Id { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string BaseUnit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public string? AllergenInfo { get; set; }
    public int? CaloriesPerUnit { get; set; }
    public decimal Quantity { get; set; }
    public decimal LineCost { get; set; }
    public decimal LineCalories { get; set; }
}
