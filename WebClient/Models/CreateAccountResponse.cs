namespace WebClient.Models;

public sealed class CreateAccountResponse
{
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}