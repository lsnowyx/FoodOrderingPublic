namespace Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public Guid MenuItemId { get; set; }
    public MenuItem MenuItem { get; set; } = null!;

    public int Quantity { get; set; }
}