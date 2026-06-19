using Application.DTOs.Common;
using Application.Features.Catalog.GetCategories;
using Application.Features.Catalog.GetMenuItemDetails;
using Application.Features.Catalog.GetMenuItemsByCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/catalog")]
[ApiController]
[AllowAnonymous]
public sealed class CatalogController : ControllerBase
{
    private readonly IMediator _mediator;

    public CatalogController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize)
    {
        var result = await _mediator.Send(new GetCatalogCategoriesQuery
        {
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("categories/{categoryId:guid}/menu-items")]
    public async Task<IActionResult> GetMenuItemsByCategory(
        Guid categoryId,
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize)
    {
        var result = await _mediator.Send(new GetCatalogMenuItemsByCategoryQuery
        {
            CategoryId = categoryId,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("menu-items/{menuItemId:guid}")]
    public async Task<IActionResult> GetMenuItemDetails(Guid menuItemId)
    {
        var result = await _mediator.Send(new GetCatalogMenuItemDetailsQuery
        {
            MenuItemId = menuItemId
        });

        return Ok(result);
    }
}
