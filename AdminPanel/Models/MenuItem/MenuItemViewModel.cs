using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AdminPanel.Models.MenuItem;

public class MenuItemViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [BindNever]
    public decimal RestaurantCost { get; set; }

    [BindNever]
    public decimal TotalCalories { get; set; }

    [Required]
    public Guid CategoryId { get; set; }

    public bool IsAvailable { get; set; } = true;

    public IEnumerable<MenuItemPictureViewModel> Pictures { get; set; } = Enumerable.Empty<MenuItemPictureViewModel>();

    // UI helper
    public string? CategoryName { get; set; }
}
