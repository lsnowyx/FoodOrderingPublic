using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AdminPanel.Models.Category;

public class CategoryViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }
}
