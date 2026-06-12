using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string? Address { get; set; }
    public List<Order> Orders { get; set; } = new();
    public Cart? Cart { get; set; }

    public List<AuditLog> AuditLogs { get; set; } = new();



    public List<Request> ClientRequest { get; set; } = new List<Request>();
    public List<Request> WorkerRequest { get; set; } = new List<Request>();
}