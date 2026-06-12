using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AdminPanel.Models.MenuItem;

public class MenuItemPictureViewModel
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; }

    [StringLength(250)]
    public string? Caption { get; set; }
}
