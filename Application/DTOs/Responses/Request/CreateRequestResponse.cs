using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Responses.Request;

public sealed record CreateRequestResponse
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public bool Completed { get; set; }

    public Guid ClientId { get; set; }
}
