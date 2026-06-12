using Application.DTOs.Request;
using Application.DTOs.Responses.Account;
using Application.Features.Account.Create;
using Application.Features.Account.Get;
using Application.Features.Account.Login;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace WebApi.Controllers;

[Route("api/[controller]/[Action]")]
[ApiController]
public class AccountsController : Controller
{
    private readonly IMediator mediator;

    public AccountsController(IMediator mediator)
    {
        this.mediator = mediator;
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginCommand loginRequest)
    {
        LoginResponse result = await mediator.Send(loginRequest);
        return Ok(result);
    }
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateUser(CreateAccountRequest createAccount)
    {
        var request = createAccount.Adapt<CreateAccountCommand>();
        request.Role = UserRoleConstants.USER_ROLE;
        CreateAccountResponse result = await mediator.Send(request);
        return Ok(result);
    }
    [HttpPost]
    [Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
    public async Task<IActionResult> CreateWorker(CreateAccountRequest createAccount)
    {
        var request = createAccount.Adapt<CreateAccountCommand>();
        request.Role = UserRoleConstants.WORKER_ROLE;
        CreateAccountResponse result = await mediator.Send(request);
        return Ok(result);
    }
    [HttpGet]
    [Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
    public async Task<IActionResult> GetWorkers()
    {
        var request = new GetAccountQuery();
        List<GetAccountReponse> result = await mediator.Send(request);
        return Ok(result);
    }
}