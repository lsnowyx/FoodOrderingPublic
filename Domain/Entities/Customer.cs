using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class Customer : IdentityUser<Guid>
{
    public string? Address { get; set; }

    public List<Order> Orders { get; set; } = new();
    public Cart? Cart { get; set; }
}
