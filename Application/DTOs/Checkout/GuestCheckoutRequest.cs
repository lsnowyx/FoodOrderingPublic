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

    [Required]
    [MinLength(1)]
    public List<GuestCheckoutItemRequest> Items { get; set; } = new();
}
