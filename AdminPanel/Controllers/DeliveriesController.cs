using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(Roles = UserRoleConstants.ORDER_MANAGER_ROLE)]
public class DeliveriesController : Controller
{
    private readonly IApiClient _apiClient;

    public DeliveriesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var order = await _apiClient.GetMyDeliveryAsync();
            return View(order);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return View(null);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Deliveries") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartDelivery(Guid id)
    {
        try
        {
            await _apiClient.StartDeliveryAsync(id);
            TempData["Success"] = "Delivery started.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDelivered(Guid id)
    {
        try
        {
            await _apiClient.MarkDeliveredAsync(id);
            TempData["Success"] = "Order marked as delivered.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
        }

        return RedirectToAction("Index");
    }
}
