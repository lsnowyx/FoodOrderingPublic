using Common.Constants;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Checkout;

public class GuestCheckoutRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(32, MinimumLength = 1)]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public string DeliveryAddress { get; set; } = null!;

    [Range(-90, 90)]
    public double? DeliveryLatitude { get; set; }

    [Range(-180, 180)]
    public double? DeliveryLongitude { get; set; }

    [Required]
    public bool PayOnline { get; set; } = false;

    [Required]
    [MinLength(1)]
    [MaxLength(OrderValidationConstants.MaxOrderLineCount)]
    public List<GuestCheckoutItemRequest> Items { get; set; } = new();
}
