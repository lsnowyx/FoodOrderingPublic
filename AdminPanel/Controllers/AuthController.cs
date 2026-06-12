using AdminPanel.Models.Auth;
using AdminPanel.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Controllers;

public class AuthController : Controller
{
    private readonly IApiClient _apiClient;

    public AuthController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var payload = new { Username = model.Username, Password = model.Password };
            var resp = await _apiClient.PostAsync<object, LoginResponseModel>("api/Accounts/Login", payload);
            if (resp == null || string.IsNullOrWhiteSpace(resp.JWT))
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Username ?? "Admin"),
                new Claim("access_token", resp.JWT)
            };

            if (!string.IsNullOrWhiteSpace(resp.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, resp.Role));
            }

            var identity = new ClaimsIdentity(claims, "AdminCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("AdminCookie", principal);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Dashboard");
        }
        catch
        {
            ModelState.AddModelError(string.Empty, "Login failed");
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AdminCookie");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
