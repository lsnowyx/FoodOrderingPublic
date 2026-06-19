using AdminPanel.Models.Order;
using AdminPanel.Services;
using Application.Features.Order.GetMyDelivery;
using Application.Features.Order.MarkDelivered;
using Application.Features.Order.StartDelivery;
using Common.Constants;
using Common.Extensions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
public class DeliveriesController : Controller
{
    private readonly IMediator _mediator;

    public DeliveriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _mediator.Send(new GetMyDeliveryQuery
        {
            OrderManagerId = User.GetUserId()
        });

        return View(response?.Adapt<OrderViewModel>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartDelivery(Guid id)
    {
        await _mediator.Send(new StartDeliveryCommand
        {
            Id = id,
            OrderManagerId = User.GetUserId()
        });

        MvcErrorHelper.SetSuccessMessage(TempData, "Delivery started.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDelivered(Guid id)
    {
        await _mediator.Send(new MarkOrderDeliveredCommand
        {
            Id = id,
            OrderManagerId = User.GetUserId()
        });

        MvcErrorHelper.SetSuccessMessage(TempData, "Order marked as delivered.");
        return RedirectToAction(nameof(Index));
    }
}
