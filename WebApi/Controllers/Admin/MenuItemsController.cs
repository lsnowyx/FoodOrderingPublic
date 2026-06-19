using Application.DTOs.Common;
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
[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class MenuItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isAvailable = null)
    {
        var result = await _mediator.Send(new GetMenuItemsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IsAvailable = isAvailable
        });

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
    public async Task<IActionResult> Create(CreateMenuItemRequest request)
    {
        var cmd = request.Adapt<CreateMenuItemCommand>();
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateMenuItemRequest request)
    {
        var cmd = request.Adapt<UpdateMenuItemCommand>();
        cmd.Id = id;
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteMenuItemCommand { Id = id });
        return Ok(result);
    }
}
