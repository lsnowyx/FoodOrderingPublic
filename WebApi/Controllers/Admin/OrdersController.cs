using Application.DTOs.Order;
using Application.Features.Order.AdjustItems;
using Application.Features.Order.Create;
using Application.Features.Order.Get;
using Application.Features.Order.GetAvailable;
using Application.Features.Order.GetById;
using Application.Features.Order.GetMyDelivery;
using Application.Features.Order.MarkDelivered;
using Application.Features.Order.RemoveItem;
using Application.Features.Order.StartDelivery;
using Application.Features.Order.Take;
using Application.Features.Order.UpdatePayment;
using Application.Features.Order.UpdateStatus;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers.Admin;

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
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetOrdersQuery());
        return Ok(result);
    }

    [HttpGet("available")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetAvailable()
    {
        var result = await _mediator.Send(new GetAvailableOrdersQuery());
        return Ok(result);
    }

    [HttpGet("my-delivery")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetMyDelivery()
    {
        var userId = Guid.Parse(User.GetUserId());
        var result = await _mediator.Send(new GetMyDeliveryQuery { OrderManagerId = userId });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var command = request.Adapt<UpdateOrderStatusCommand>();
        command.Id = id;
        command.OrderManagerId = Guid.Parse(User.GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/payment")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdateOrderPaymentRequest request)
    {
        var command = request.Adapt<UpdateOrderPaymentCommand>();
        command.Id = id;
        command.OrderManagerId = Guid.Parse(User.GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id:guid}/items")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> AdjustItems(Guid id, [FromBody] AdjustOrderItemsRequest request)
    {
        var command = request.Adapt<AdjustOrderItemsCommand>();
        command.OrderId = id;
        command.OrderManagerId = Guid.Parse(User.GetUserId());
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId)
    {
        var result = await _mediator.Send(new RemoveOrderItemCommand { OrderId = id, ItemId = itemId, OrderManagerId = Guid.Parse(User.GetUserId()) });
        return Ok(result);
    }

    [HttpPatch("{id:guid}/mark-delivered")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> MarkDelivered(Guid id)
    {
        var result = await _mediator.Send(new MarkOrderDeliveredCommand { Id = id, OrderManagerId = Guid.Parse(User.GetUserId()) });
        return Ok(result);
    }

    [HttpPatch("{id:guid}/take")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> Take(Guid id)
    {
        var result = await _mediator.Send(new TakeOrderCommand { OrderId = id, OrderManagerId = Guid.Parse(User.GetUserId()) });
        return Ok(result);
    }

    [HttpPatch("{id:guid}/start-delivery")]
    [Authorize(AuthorizationPolicyConstants.ORDER_MANAGER_POLICY)]
    public async Task<IActionResult> StartDelivery(Guid id)
    {
        var result = await _mediator.Send(new StartDeliveryCommand { Id = id, OrderManagerId = Guid.Parse(User.GetUserId()) });
        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
    {
        var cmd = request.Adapt<CreateOrderCommand>();
        cmd.CustomerId = Guid.Parse(User.GetUserId());

        var created = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
