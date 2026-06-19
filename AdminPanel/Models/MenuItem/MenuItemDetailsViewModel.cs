using System.Collections.Generic;
using System.Linq;

namespace AdminPanel.Models.MenuItem;

public class MenuItemDetailsViewModel
{
    public MenuItemViewModel Item { get; set; } = new MenuItemViewModel();
    public IEnumerable<MenuItemIngredientViewModel> Ingredients { get; set; } = Enumerable.Empty<MenuItemIngredientViewModel>();
    public IEnumerable<MenuItemPictureViewModel> Pictures { get; set; } = Enumerable.Empty<MenuItemPictureViewModel>();
}
