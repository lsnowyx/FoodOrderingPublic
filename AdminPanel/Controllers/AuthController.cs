using AdminPanel.Constants;
using AdminPanel.Models.Auth;
using Application.DTOs.Request;
using Application.Features.Account.Login;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminPanel.Controllers;

public class AuthController : Controller
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var request = model.Adapt<LoginRequest>();
        if (!TryValidateModel(request))
        {
            return View(model);
        }

        try
        {
            var command = request.Adapt<LoginCommand>();
            var response = await _mediator.Send(command);

            if (!IsAllowedStaffRole(response.Role))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account is not permitted to access the AdminPanel.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.UserId.ToString()),
                new(ClaimTypes.Name, request.Username),
                new(ClaimTypes.Role, response.Role)
            };

            var identity = new ClaimsIdentity(claims, AdminAuthenticationConstants.CookieScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(AdminAuthenticationConstants.CookieScheme, principal);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
                && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(DashboardController.Index), "Dashboard");
        }
        catch (UnauthorizedAccessException)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }
        catch (InvalidOperationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(model);
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AdminAuthenticationConstants.CookieScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private static bool IsAllowedStaffRole(string role)
    {
        return UserRoleConstants.ADMIN_PANEL_ROLES.Contains(role, StringComparer.Ordinal);
    }
}
