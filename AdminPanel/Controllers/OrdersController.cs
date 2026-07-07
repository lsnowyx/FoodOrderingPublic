using AdminPanel.Models.Order;
using AdminPanel.Services;
using Application.DTOs.Common;
using Application.DTOs.Order;
using Application.Features.Order.GetAssignedById;
using Application.Features.Order.GetAvailable;
using Application.Features.Order.Take;
using Application.Features.Order.UpdatePayment;
using Application.Features.Order.UpdateStatus;
using Common.Constants;
using Common.Enums;
using Common.Extensions;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
public class OrdersController : Controller
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(
        int page = PaginationParameters.DefaultPage,
        int pageSize = PaginationParameters.DefaultPageSize,
        OrderStatus? status = null,
        bool? isPaid = null)
    {
        var response = await _mediator.Send(new GetAvailableOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            IsPaid = isPaid
        });

        var model = new OrderIndexViewModel
        {
            Status = status,
            IsPaid = isPaid,
            Orders = response.Adapt<AdminPanel.Models.Common.PaginatedViewModel<OrderViewModel>>()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Take(Guid id)
    {
        await _mediator.Send(new TakeOrderCommand
        {
            OrderId = id,
            OrderManagerId = User.GetUserId()
        });

        MvcErrorHelper.SetSuccessMessage(TempData, "Order assigned to you.");
        return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var response = await _mediator.Send(new GetAssignedOrderByIdQuery
        {
            Id = id,
            OrderManagerId = User.GetUserId()
        });

        return response is null
            ? NotFound()
            : View(response.Adapt<OrderViewModel>());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(Guid id, UpdateOrderPaymentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            MvcErrorHelper.SetFirstModelStateErrorMessage(
                TempData,
                ModelState,
                "Payment status is invalid.");
            return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
        }

        var request = model.Adapt<UpdateOrderPaymentRequest>();
        if (!this.ValidateRequestDtoForRedirect(request, "Payment status is invalid."))
        {
            return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
        }

        var command = request.Adapt<UpdateOrderPaymentCommand>();
        command.Id = id;
        command.OrderManagerId = User.GetUserId();

        await _mediator.Send(command);
        MvcErrorHelper.SetSuccessMessage(TempData, "Payment status updated.");
        return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusViewModel model)
    {
        if (!ModelState.IsValid)
        {
            MvcErrorHelper.SetFirstModelStateErrorMessage(
                TempData,
                ModelState,
                "Order status is invalid.");
            return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
        }

        var request = model.Adapt<UpdateOrderStatusRequest>();
        if (!this.ValidateRequestDtoForRedirect(request, "Order status is invalid."))
        {
            return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
        }

        var command = request.Adapt<UpdateOrderStatusCommand>();
        command.Id = id;
        command.OrderManagerId = User.GetUserId();

        await _mediator.Send(command);
        MvcErrorHelper.SetSuccessMessage(TempData, "Order status updated.");
        return RedirectToAction(nameof(DeliveriesController.Index), "Deliveries");
    }

}
