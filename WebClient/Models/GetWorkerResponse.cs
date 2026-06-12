namespace WebClient.Models;

public sealed class GetWorkerResponse
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
}