using Application.DTOs.MenuItemIngredient;
using Application.Features.MenuItemIngredient.Create;
using Application.Features.MenuItemIngredient.Delete;
using Application.Features.MenuItemIngredient.Get;
using Application.Features.MenuItemIngredient.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Admin;

[Route("api/menu-items/{menuItemId:guid}/ingredients")]
[ApiController]
[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class MenuItemIngredientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemIngredientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid menuItemId)
    {
        var result = await _mediator.Send(new GetMenuItemIngredientsQuery { MenuItemId = menuItemId });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid menuItemId, CreateMenuItemIngredientRequest request)
    {
        var cmd = request.Adapt<CreateMenuItemIngredientCommand>();
        cmd.MenuItemId = menuItemId;
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(Get), new { menuItemId }, result);
    }

    [HttpPut("{ingredientId:guid}")]
    public async Task<IActionResult> Update(Guid menuItemId, Guid ingredientId, UpdateMenuItemIngredientRequest request)
    {
        var cmd = request.Adapt<UpdateMenuItemIngredientCommand>();
        cmd.MenuItemId = menuItemId;
        cmd.IngredientId = ingredientId;
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{ingredientId:guid}")]
    public async Task<IActionResult> Delete(Guid menuItemId, Guid ingredientId)
    {
        var result = await _mediator.Send(new DeleteMenuItemIngredientCommand { MenuItemId = menuItemId, IngredientId = ingredientId });
        return Ok(result);
    }
}
