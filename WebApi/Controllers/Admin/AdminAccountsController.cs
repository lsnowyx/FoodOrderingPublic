using Application.DTOs.Request;
using Application.Features.Account.CreateStaff;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Admin;

[Route("api/admin/accounts")]
[ApiController]
[Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
public class AdminAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAdminAccountRequest request)
    {
        var command = request.Adapt<CreateStaffAccountCommand>();
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateAccount), result);
    }
}
