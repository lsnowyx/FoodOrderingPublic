using Application.DTOs.Checkout;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.Checkout.Guest;

public class GuestCheckoutCommand : IRequest<GuestCheckoutResponse>
{
    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public double? DeliveryLatitude { get; set; }

    public double? DeliveryLongitude { get; set; }

    public bool PayOnline { get; set; } = false;

    public List<GuestCheckoutItemCommand> Items { get; set; } = new();
}
