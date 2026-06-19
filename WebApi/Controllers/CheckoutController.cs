using Application.DTOs.Checkout;
using Application.Features.Checkout.Guest;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/checkout")]
[ApiController]
public class CheckoutController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckoutController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("guest")]
    [AllowAnonymous]
    public async Task<IActionResult> Guest([FromBody] GuestCheckoutRequest request)
    {
        var command = request.Adapt<GuestCheckoutCommand>();
        var response = await _mediator.Send(command);

        return Ok(response);
    }
}
