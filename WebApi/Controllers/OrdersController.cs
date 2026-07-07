using Application.DTOs.Common;
using Application.DTOs.Order;
using Application.Features.Order.AdjustItems;
using Application.Features.Order.Create;
using Application.Features.Order.Get;
using Application.Features.Order.GetAssignedById;
using Application.Features.Order.GetCustomerDeliveryTrackingMap;
using Application.Features.Order.GetCustomerOrderDetails;
using Application.Features.Order.GetCustomerOrders;
using Application.Features.Order.GetCustomerPaymentLink;
using Application.Features.Order.GetAvailable;
using Application.Features.Order.GetMyDelivery;
using Application.Features.Order.MarkDelivered;
using Application.Features.Order.RemoveItem;
using Application.Features.Order.StartDelivery;
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

namespace WebApi.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] bool? isPaid = null)
    {
        var result = await _mediator.Send(new GetOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            IsPaid = isPaid
        });
        return Ok(result);
    }

    [HttpGet("available")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] bool? isPaid = null)
    {
        var result = await _mediator.Send(new GetAvailableOrdersQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status,
            IsPaid = isPaid
        });
        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize)
    {
        var result = await _mediator.Send(new GetCustomerOrdersQuery
        {
            CustomerId = User.GetUserId(),
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("my/{orderId:guid}")]
    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task<IActionResult> GetMyOrderById(Guid orderId)
    {
        var result = await _mediator.Send(new GetCustomerOrderDetailsQuery
        {
            OrderId = orderId,
            CustomerId = User.GetUserId()
        });

        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("my/{orderId:guid}/map")]
    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task<IActionResult> GetMyOrderMap(Guid orderId)
    {
        var result = await _mediator.Send(new GetCustomerDeliveryTrackingMapQuery
        {
            OrderId = orderId,
            CustomerId = User.GetUserId()
        });

        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("my/{orderId:guid}/payment-link")]
    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task<IActionResult> GetMyOrderPaymentLink(Guid orderId)
    {
        var result = await _mediator.Send(new GetCustomerPaymentLinkCommand
        {
            OrderId = orderId,
            CustomerId = User.GetUserId()
        });

        return Ok(result);
    }

    [HttpGet("my-delivery")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetMyDelivery()
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetMyDeliveryQuery { OrderManagerId = userId });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{orderId:guid}")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetById(Guid orderId)
    {
        var result = await _mediator.Send(new GetAssignedOrderByIdQuery
        {
            Id = orderId,
            OrderManagerId = User.GetUserId()
        });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{orderId:guid}/status")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> UpdateStatus(Guid orderId, [FromBody] UpdateOrderStatusRequest request)
    {
        var command = request.Adapt<UpdateOrderStatusCommand>();
        command.Id = orderId;
        command.OrderManagerId = User.GetUserId();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("{orderId:guid}/payment")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> UpdatePayment(Guid orderId, [FromBody] UpdateOrderPaymentRequest request)
    {
        var command = request.Adapt<UpdateOrderPaymentCommand>();
        command.Id = orderId;
        command.OrderManagerId = User.GetUserId();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{orderId:guid}/items")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> AdjustItems(Guid orderId, [FromBody] AdjustOrderItemsRequest request)
    {
        var command = request.Adapt<AdjustOrderItemsCommand>();
        command.OrderId = orderId;
        command.OrderManagerId = User.GetUserId();
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{orderId:guid}/items/{itemId:guid}")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> RemoveItem(Guid orderId, Guid itemId)
    {
        var result = await _mediator.Send(new RemoveOrderItemCommand
        {
            OrderId = orderId,
            ItemId = itemId,
            OrderManagerId = User.GetUserId()
        });
        return Ok(result);
    }

    [HttpPatch("{orderId:guid}/mark-delivered")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> MarkDelivered(Guid orderId)
    {
        var result = await _mediator.Send(new MarkOrderDeliveredCommand { Id = orderId, OrderManagerId = User.GetUserId() });
        return Ok(result);
    }

    [HttpPatch("{orderId:guid}/take")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> Take(Guid orderId)
    {
        var result = await _mediator.Send(new TakeOrderCommand { OrderId = orderId, OrderManagerId = User.GetUserId() });
        return Ok(result);
    }

    [HttpPatch("{orderId:guid}/start-delivery")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> StartDelivery(Guid orderId)
    {
        var result = await _mediator.Send(new StartDeliveryCommand { Id = orderId, OrderManagerId = User.GetUserId() });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var cmd = request.Adapt<CreateOrderCommand>();
        cmd.UserId = User.GetUserId();

        var created = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { orderId = created.OrderId }, created);
    }
}
