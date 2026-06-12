using AdminPanel.Models.Dashboard;
using AdminPanel.Models.Order;
using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IApiClient _apiClient;

    public DashboardController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(UserRoleConstants.ORDER_MANAGER_ROLE)
            && !User.IsInRole(UserRoleConstants.ADMIN_ROLE)
            && !User.IsInRole(UserRoleConstants.MENU_MANAGER_ROLE))
        {
            return RedirectToAction("Index", "Deliveries");
        }

        var vm = new DashboardViewModel
        {
            IsMenuManager = User.IsInRole(UserRoleConstants.MENU_MANAGER_ROLE),
            IsOrderManager = User.IsInRole(UserRoleConstants.ORDER_MANAGER_ROLE),
            UserName = User.Identity?.Name ?? "Admin"
        };

        if (vm.IsOrderManager)
        {
            try
            {
                // call WebApi orders endpoint - use strongly-typed OrderViewModel
                var orders = await _apiClient.GetAsync<IEnumerable<OrderViewModel>>("api/orders");
                if (orders != null)
                {
                    var list = orders.Select(o => new OrderSummaryViewModel
                    {
                        Id = o.Id,
                        OrderDate = o.OrderDate,
                        Status = o.Status,
                        IsPaid = o.IsPaid
                    }).ToList();

                    vm.TotalOrders = list.Count;
                    vm.PendingOrders = list.Count(x => x.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));
                    vm.PaidOrders = list.Count(x => x.IsPaid);
                    vm.UnpaidOrders = list.Count(x => !x.IsPaid);
                    vm.PreparingOrders = list.Count(x => x.Status.Equals("Preparing", StringComparison.OrdinalIgnoreCase));
                    vm.OutForDeliveryOrders = list.Count(x => x.Status.Equals("OutForDelivery", StringComparison.OrdinalIgnoreCase) || x.Status.Equals("OutForDelivery", StringComparison.OrdinalIgnoreCase));
                    vm.DeliveredOrders = list.Count(x => x.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase));
                    vm.CancelledOrders = list.Count(x => x.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase));
                    vm.RecentOrders = list.Take(10).ToList();
                }
                else
                {
                    ViewData["ApiError"] = "Orders API returned no data.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index") });
            }
            catch (ForbiddenException)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            catch
            {
                ViewData["ApiError"] = "Failed to fetch orders from API.";
            }
        }

        return View(vm);
    }
}
