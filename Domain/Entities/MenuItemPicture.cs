namespace Domain.Entities;

public class MenuItemPicture
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;
    public string ImagePublicId { get; set; } = null!;

    public string? Caption { get; set; }
}