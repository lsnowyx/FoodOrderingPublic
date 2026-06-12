using System.ComponentModel.DataAnnotations;

namespace WebClient.Models;

public sealed class CreateAccountRequest
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string UserName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must contain at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
}