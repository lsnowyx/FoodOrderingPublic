using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class MenuItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal RestaurantCost { get; set; }

    public decimal TotalCalories { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsAvailable { get; set; } = true;

    public List<MenuItemIngredient> MenuItemIngredients { get; set; } = new();
    public List<MenuItemPicture> MenuItemPictures { get; set; } = new();
}
