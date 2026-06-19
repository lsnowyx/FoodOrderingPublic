using Application.DTOs.Category;
using Application.DTOs.Common;
using Application.Features.Category.Create;
using Application.Features.Category.Delete;
using Application.Features.Category.Get;
using Application.Features.Category.GetById;
using Application.Features.Category.Lookup;
using Application.Features.Category.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Admin;

[Route("api/categories")]
[ApiController]
[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetCategoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        return Ok(result);
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.LookupPageSize)
    {
        var result = await _mediator.Send(new GetCategoryLookupQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateCategoryRequest request)
    {
        var cmd = request.Adapt<CreateCategoryCommand>();
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateCategoryRequest request)
    {
        var cmd = request.Adapt<UpdateCategoryCommand>();
        cmd.Id = id;
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand { Id = id });
        return Ok(result);
    }
}
