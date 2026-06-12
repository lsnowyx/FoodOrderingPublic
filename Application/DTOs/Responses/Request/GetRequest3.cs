using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Responses.Request;

public class GetRequest3
{
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public bool Completed { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string? WorkerName { get; set; }

    public Guid ClientId { get; set; }

    public Guid? WorkerId { get; set; }
}
