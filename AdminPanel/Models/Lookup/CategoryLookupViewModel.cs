namespace AdminPanel.Models.Lookup;

public class CategoryLookupViewModel
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Description { get; set; }
}
