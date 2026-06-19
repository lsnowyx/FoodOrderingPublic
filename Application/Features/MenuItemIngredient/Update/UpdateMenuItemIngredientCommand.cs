using Application.DTOs.MenuItemIngredient;
using MediatR;

namespace Application.Features.MenuItemIngredient.Update;

using System.ComponentModel.DataAnnotations;

public sealed class UpdateMenuItemIngredientCommand : IRequest<MenuItemIngredientResponse>
{
    public Guid MenuItemId { get; set; }

    [Required]
    public Guid IngredientId { get; set; }

    [Range(typeof(decimal), "0.0001", "99999999999999.9999")]
    public decimal Quantity { get; set; }
}
