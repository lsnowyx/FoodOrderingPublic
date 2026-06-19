namespace Domain.Entities;

public class MenuItemIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;

    public decimal Quantity { get; set; }
}
