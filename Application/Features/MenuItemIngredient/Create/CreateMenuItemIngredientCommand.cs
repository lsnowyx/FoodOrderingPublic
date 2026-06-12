using Application.DTOs.MenuItemIngredient;
using MediatR;

namespace Application.Features.MenuItemIngredient.Create;

using System.ComponentModel.DataAnnotations;

public sealed class CreateMenuItemIngredientCommand : IRequest<MenuItemIngredientResponse>
{
    public Guid MenuItemId { get; set; }

    [Required]
    public Guid IngredientId { get; set; }

    [Required]
    [StringLength(50)]
    public string Quantity { get; set; } = null!;
}
