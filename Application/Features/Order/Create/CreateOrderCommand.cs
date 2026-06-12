using Application.DTOs.Order;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Order.Create;

public sealed class CreateOrderCommand : IRequest<Application.DTOs.Order.OrderResponse>
{
    public Guid CustomerId { get; set; }

    [Required]
    public string DeliveryAddress { get; set; } = null!;

    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}
