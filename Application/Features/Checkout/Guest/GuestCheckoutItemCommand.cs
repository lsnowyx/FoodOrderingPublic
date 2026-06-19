namespace Application.Features.Checkout.Guest;

public class GuestCheckoutItemCommand
{
    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }
}
