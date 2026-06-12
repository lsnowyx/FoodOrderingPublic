namespace Domain.Entities;

public class Cart
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public List<CartItem> CartItems { get; set; } = new();
}