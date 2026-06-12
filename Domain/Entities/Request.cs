using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Request
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    public bool Completed { get; set; } = false;

    public Guid ClientId { get; set; }
    public User Client { get; set; } = null!;

    public Guid? WorkerId { get; set; }
    public User? Worker { get; set; }
}