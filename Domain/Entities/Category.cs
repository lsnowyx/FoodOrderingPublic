using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }
    public string? ImagePublicId { get; set; }

    public List<MenuItem> MenuItems { get; set; } = new();
}