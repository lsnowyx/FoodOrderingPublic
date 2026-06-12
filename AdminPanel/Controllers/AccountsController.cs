using AdminPanel.Models.Auth;
using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(Roles = UserRoleConstants.ADMIN_ROLE)]
public class AccountsController : Controller
{
    private readonly IApiClient _apiClient;

    public AccountsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IActionResult Create()
    {
        return View(new CreateAccountViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var payload = new
            {
                userName = model.UserName,
                password = model.Password,
                role = model.Role
            };

            await _apiClient.PostAsync<object, object>("api/admin/accounts", payload);
            TempData["Success"] = $"Account created successfully for {model.UserName}";
            return RedirectToAction("Create");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Create", "Accounts") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
        catch (ApiException ex)
        {
            ApiErrorHelper.AddErrorsToModelState(ModelState, ex.Content);
            return View(model);
        }
    }
}
