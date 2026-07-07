using Application.DTOs.Order;
using Common.Constants;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Order.Create;

public sealed class CreateOrderCommand : IRequest<CreateOrderResponse>
{
    public Guid? UserId { get; set; }

    [Required]
    public string DeliveryAddress { get; set; } = null!;

    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }

    public bool PayOnline { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(OrderValidationConstants.MaxOrderLineCount)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
