using System;
using System.Collections.Generic;

namespace AdminPanel.Models.MenuItem;

public class MenuItemIngredientViewModel
{
    public Guid Id { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string? AllergenInfo { get; set; }
    public int? CaloriesPerUnit { get; set; }
    public string Quantity { get; set; } = string.Empty;
}
