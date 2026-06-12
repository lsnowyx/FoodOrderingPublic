using Domain.Entities;

namespace Domain.ConfigModels;

public class SeedUser : User
{
    public string Password { get; set; } = string.Empty;
    public IEnumerable<string> Roles { get; set; } = new List<string>();
}