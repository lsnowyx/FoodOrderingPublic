namespace Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string HttpMethod { get; set; } = null!;

    public string Endpoint { get; set; } = null!;

    public int StatusCode { get; set; }

    public string? RequestBody { get; set; }

    public string? ResponseBody { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}