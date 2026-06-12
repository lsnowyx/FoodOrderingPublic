using Application.DTOs.MenuItem;
using MediatR;

namespace Application.Features.MenuItem.Update;

using System.ComponentModel.DataAnnotations;

public sealed class UpdateMenuItemCommand : IRequest<MenuItemResponse>
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public Guid CategoryId { get; set; }

    public bool IsAvailable { get; set; }
}
