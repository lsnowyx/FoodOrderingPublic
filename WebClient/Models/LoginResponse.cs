using System.Text.Json.Serialization;

namespace WebClient.Models;

public sealed class LoginResponse
{
    [JsonPropertyName("jwt")]
    public string JWT { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}