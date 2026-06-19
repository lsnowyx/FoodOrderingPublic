using AdminPanel.Models.Common;

namespace AdminPanel.Models.Category;

public class CategoryIndexViewModel
{
    public string? SearchTerm { get; set; }
    public PaginatedViewModel<CategoryViewModel> Categories { get; set; } = new();
}
