using Application.DTOs.MenuItemPicture;
using Application.Features.MenuItemPicture.Create;
using Application.Features.MenuItemPicture.Delete;
using Application.Features.MenuItemPicture.Get;
using Application.Features.MenuItemPicture.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Admin;

[Route("api/menu-items/{menuItemId:guid}/pictures")]
[ApiController]
[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class MenuItemPicturesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MenuItemPicturesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid menuItemId)
    {
        var result = await _mediator.Send(new GetMenuItemPicturesQuery { MenuItemId = menuItemId });
        return Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(Guid menuItemId, [FromForm] MenuItemPictureRequest request)
    {
        var cmd = request.Adapt<CreateMenuItemPictureCommand>();
        cmd.MenuItemId = menuItemId;
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(Get), new { menuItemId }, result);
    }

    [HttpPut("{pictureId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid menuItemId, Guid pictureId, [FromForm] MenuItemPictureRequest request)
    {
        var cmd = new UpdateMenuItemPictureCommand { MenuItemId = menuItemId, PictureId = pictureId, ImageFile = request.ImageFile, Caption = request.Caption };
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{pictureId:guid}")]
    public async Task<IActionResult> Delete(Guid menuItemId, Guid pictureId)
    {
        var result = await _mediator.Send(new DeleteMenuItemPictureCommand { MenuItemId = menuItemId, PictureId = pictureId });
        return Ok(result);
    }
}
