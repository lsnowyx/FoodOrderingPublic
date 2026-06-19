using AdminPanel.Models.Dashboard;
using Application.Features.Dashboard.GetSummary;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var isAdmin = User.IsInRole(UserRoleConstants.ADMIN_ROLE);
        var isMenuManager = User.IsInRole(UserRoleConstants.MENU_MANAGER_ROLE);
        var isOrderManager = User.IsInRole(UserRoleConstants.ORDER_MANAGER_ROLE);

        if (isOrderManager && !isAdmin && !isMenuManager)
        {
            return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
        }

        var showOrderSummary = isAdmin || isOrderManager;
        DashboardViewModel model;

        if (showOrderSummary)
        {
            var response = await _mediator.Send(new GetDashboardSummaryQuery());
            model = response.Adapt<DashboardViewModel>();
        }
        else
        {
            model = new DashboardViewModel();
        }

        model.IsMenuManager = isMenuManager;
        model.IsOrderManager = isOrderManager;
        model.ShowOrderSummary = showOrderSummary;
        model.UserName = User.Identity?.Name ?? string.Empty;

        return View(model);
    }
}
