using AdminPanel.Models.Order;
using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(Roles = UserRoleConstants.ORDER_MANAGER_ROLE)]
public class OrdersController : Controller
{
    private readonly IApiClient _apiClient;

    public OrdersController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(string? status, string? paid)
    {
        try
        {
            var list = await _apiClient.GetAvailableOrdersAsync();
            var items = (list ?? Enumerable.Empty<OrderViewModel>());
            if (!string.IsNullOrWhiteSpace(status))
            {
                items = items.Where(i => string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrWhiteSpace(paid))
            {
                if (paid.Equals("paid", StringComparison.OrdinalIgnoreCase)) items = items.Where(i => i.IsPaid);
                if (paid.Equals("unpaid", StringComparison.OrdinalIgnoreCase)) items = items.Where(i => !i.IsPaid);
            }
            return View(items);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Orders") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return View(Enumerable.Empty<OrderViewModel>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(Guid id)
    {
        try
        {
            await _apiClient.TakeOrderAsync(id);
            TempData["Success"] = "Order assigned to you.";
            return RedirectToAction("Index", "Deliveries");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Index");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Orders") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<OrderViewModel>($"api/orders/{id}");
            if (item == null) return NotFound();
            return View(item);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Orders", new { id }) });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(Guid id, bool isPaid)
    {
        try
        {
            var payload = new UpdateOrderPaymentViewModel { IsPaid = isPaid };
            await _apiClient.PatchAsync<object, object>($"api/orders/{id}/payment", payload);
            TempData["Success"] = "Payment status updated.";
            return RedirectToAction("Index", "Deliveries");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Index", "Deliveries");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Orders", new { id }) });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        try
        {
            var payload = new UpdateOrderStatusViewModel { Status = status };
            await _apiClient.PatchAsync<object, object>($"api/orders/{id}/status", payload);
            TempData["Success"] = "Order status updated.";
            return RedirectToAction("Index", "Deliveries");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Index", "Deliveries");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Orders", new { id }) });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustItems(Guid id, List<OrderItemViewModel> items)
    {
        try
        {
            var payload = new AdjustOrderItemsViewModel
            {
                Items = items.Select(i => new AdjustOrderItemViewModel { ItemId = i.Id, Quantity = i.Quantity }).ToList()
            };

            await _apiClient.PutAsync<object, object>($"api/orders/{id}/items", payload);
            TempData["Success"] = "Order items updated.";
            return RedirectToAction("Index", "Deliveries");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Index", "Deliveries");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Orders", new { id }) });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        try
        {
            await _apiClient.DeleteAsync($"api/orders/{id}/items/{itemId}");
            TempData["Success"] = "Item removed from order.";
            return RedirectToAction("Index", "Deliveries");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Index", "Deliveries");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Orders", new { id }) });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDelivered(Guid id)
    {
        try
        {
            await _apiClient.MarkDeliveredAsync(id);
            TempData["Success"] = "Order marked as delivered.";
            return RedirectToAction("Index", "Deliveries");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Index", "Deliveries");
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Details", "Orders", new { id }) });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
    }
}

