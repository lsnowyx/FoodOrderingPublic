using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models.Auth;

public class CreateAccountViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, MinimumLength = 1)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(256, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = string.Empty;
}
