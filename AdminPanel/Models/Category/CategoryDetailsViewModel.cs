using AdminPanel.Models.Common;
using AdminPanel.Models.MenuItem;

namespace AdminPanel.Models.Category;

public class CategoryDetailsViewModel
{
    public CategoryViewModel Category { get; set; } = new();
    public PaginatedViewModel<MenuItemViewModel> MenuItems { get; set; } = new();
}
