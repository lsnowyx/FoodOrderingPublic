using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public class CreateOrderItemDto
{
    [Required]
    public Guid MenuItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
