namespace WebClient.Models;

public sealed class GetRequestResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? WorkerName { get; set; }
    public Guid ClientId { get; set; }
    public Guid? WorkerId { get; set; }
}