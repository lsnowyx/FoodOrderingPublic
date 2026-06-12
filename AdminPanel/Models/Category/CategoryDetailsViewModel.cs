using System.Collections.Generic;
using System.Linq;

namespace AdminPanel.Models.Category;

public class CategoryDetailsViewModel
{
    public CategoryViewModel Category { get; set; } = new CategoryViewModel();
    public IEnumerable<AdminPanel.Models.MenuItem.MenuItemViewModel> MenuItems { get; set; } = Enumerable.Empty<AdminPanel.Models.MenuItem.MenuItemViewModel>();
}
