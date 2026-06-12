using Application.DTOs.MenuItem;
using Application.Features.MenuItem.Create;
using Application.Features.MenuItem.Delete;
using Application.Features.MenuItem.Get;
using Application.Features.MenuItem.GetById;
using Application.Features.MenuItem.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Admin;

[Route("api/menu-items")]
[ApiController]
public class MenuItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetMenuItemsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetMenuItemByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
    public async Task<IActionResult> Create(CreateMenuItemRequest request)
    {
        var cmd = request.Adapt<CreateMenuItemCommand>();
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
    public async Task<IActionResult> Update(Guid id, UpdateMenuItemRequest request)
    {
        var cmd = request.Adapt<UpdateMenuItemCommand>();
        cmd.Id = id;
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteMenuItemCommand { Id = id });
        return Ok(result);
    }
}
