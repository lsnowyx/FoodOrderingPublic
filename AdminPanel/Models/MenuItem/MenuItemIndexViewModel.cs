using AdminPanel.Models.Category;
using AdminPanel.Models.Common;

namespace AdminPanel.Models.MenuItem;

public class MenuItemIndexViewModel
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public IReadOnlyList<CategoryViewModel> Categories { get; set; } = Array.Empty<CategoryViewModel>();
    public PaginatedViewModel<MenuItemViewModel> MenuItems { get; set; } = new();
}
