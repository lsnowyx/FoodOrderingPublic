namespace WebClient.Models;

public sealed class CreateRequestResponse
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Completed { get; set; }
    public Guid ClientId { get; set; }
}