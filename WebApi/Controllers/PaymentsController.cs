using Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace WebApi.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IStripePaymentService stripePaymentService;

    public PaymentsController(IStripePaymentService stripePaymentService)
    {
        this.stripePaymentService = stripePaymentService;
    }

    [HttpPost("stripe/webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken cancellationToken)
    {
        var json = await new StreamReader(HttpContext.Request.Body)
            .ReadToEndAsync(cancellationToken);

        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(stripeSignature))
        {
            return BadRequest();
        }

        try
        {
            await stripePaymentService.HandleWebhookAsync(
                json,
                stripeSignature,
                cancellationToken);
        }
        catch (StripeException)
        {
            return BadRequest();
        }

        return Ok();
    }
}