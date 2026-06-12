namespace AdminPanel.Models.Auth;

public class LoginResponseModel
{
    public string JWT { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
