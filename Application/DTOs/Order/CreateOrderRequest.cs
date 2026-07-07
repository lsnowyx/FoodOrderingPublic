using Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Order;

public class CreateOrderRequest
{

    [Required]
    [MinLength(1)]
    public string DeliveryAddress { get; set; } = null!;

    [Range(-90, 90)]
    public double? DeliveryLatitude { get; set; }

    [Range(-180, 180)]
    public double? DeliveryLongitude { get; set; }

    public bool PayOnline { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(OrderValidationConstants.MaxOrderLineCount)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
