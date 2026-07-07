using Application.DTOs.Common;
using Application.DTOs.Ingredient;
using Application.Features.Ingredient.Create;
using Application.Features.Ingredient.Delete;
using Application.Features.Ingredient.Get;
using Application.Features.Ingredient.GetById;
using Application.Features.Ingredient.Lookup;
using Application.Features.Ingredient.NutritionSearch;
using Application.Features.Ingredient.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.Admin;

[Route("api/ingredients")]
[ApiController]
[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class IngredientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public IngredientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = PaginationParameters.DefaultPage,
        [FromQuery] int pageSize = PaginationParameters.DefaultPageSize,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetIngredientsQuery
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
        var result = await _mediator.Send(new GetIngredientLookupQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("nutrition-search")]
    public async Task<IActionResult> NutritionSearch(
        [FromQuery] string query,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new SearchNutritionQuery
        {
            Query = query,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetIngredientByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIngredientRequest request)
    {
        var cmd = request.Adapt<CreateIngredientCommand>();
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateIngredientRequest request)
    {
        var cmd = request.Adapt<UpdateIngredientCommand>();
        cmd.Id = id;
        var result = await _mediator.Send(cmd);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteIngredientCommand { Id = id });
        return Ok(result);
    }
}
