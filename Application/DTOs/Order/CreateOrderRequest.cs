using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public class CreateOrderRequest
{

    [Required]
    public string DeliveryAddress { get; set; } = null!;

    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
