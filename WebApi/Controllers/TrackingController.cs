using Application.DTOs.Tracking;
using Application.Features.Tracking.Get;
using Application.Features.Tracking.GetMap;
using Application.Features.Tracking.PaymentUrlStatus;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/tracking")]
[ApiController]
public class TrackingController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrackingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] GetOrderTrackingRequest request)
    {
        var result = await _mediator.Send(request.Adapt<GetOrderTrackingQuery>());

        return Ok(result);
    }

    [HttpGet("map")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMap([FromQuery] GetOrderTrackingRequest request)
    {
        var result = await _mediator.Send(new GetDeliveryTrackingMapQuery
        {
            Token = request.Token
        });

        return Ok(result);
    }

    [HttpPost("guest-orders/payment-link")]
    public async Task<IActionResult> GetOrCreatePaymentLink([FromQuery] GetOrderTrackingRequest request)
    {
        var result = await _mediator.Send(new PaymentUrlStatusOrderTrackingCommand(request.Token));

        return Ok(result);
    }
}
