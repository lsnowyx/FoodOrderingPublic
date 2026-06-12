using Application.DTOs.Request;
using Application.DTOs.Responses.Request;
using Application.Features.Request.Assign;
using Application.Features.Request.Complete;
using Application.Features.Request.Create;
using Application.Features.Request.Get;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;


[Route("api/[controller]/[Action]")]
[ApiController]
public class RequestsController : Controller
{
    private readonly IMediator mediator;

    public RequestsController(IMediator mediator)
    {
        this.mediator = mediator;
    }
    [HttpPost]
    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task<IActionResult> CreateRequest(CreateRequestRequest createRequest)
    {
        var request = createRequest.Adapt<CreateRequestCommand>();
        request.ClientId = Guid.Parse(User.GetUserId());
        CreateRequestResponse result = await mediator.Send(request);
        return Ok(result);
    }



    [HttpPatch("{requestId:guid}/assign/{workerId:guid}")]
    [Authorize(AuthorizationPolicyConstants.ADMIN_POLICY)]
    public async Task<IActionResult> AssignRequest([FromRoute] Guid requestId, [FromRoute] Guid workerId)
    {
        var request = new AssignRequestCommand(workerId, requestId);

        AssignRequestResponse result = await mediator.Send(request);
        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRequests()
    {
        List<GetRequest3> result = await mediator.Send(new GetRequestCommand(User.GetRole(), Guid.Parse(User.GetUserId())));
        return Ok(result);
    }

    [HttpPatch("{requestId:guid}")]
    [Authorize(AuthorizationPolicyConstants.WORKER_POLICY)]
    public async Task<IActionResult> CompleteRequest(Guid requestId)
    {
        var request = new CompleteRequestCommand(Guid.Parse(User.GetUserId()), requestId);

        GetRequestReponse result = await mediator.Send(request);
        return Ok(result);
    }
}