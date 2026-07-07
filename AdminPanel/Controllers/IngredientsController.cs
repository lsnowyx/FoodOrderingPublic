using AdminPanel.Models.Ingredient;
using AdminPanel.Services;
using Application.DTOs.Common;
using Application.DTOs.Ingredient;
using Application.Features.Ingredient.Create;
using Application.Features.Ingredient.Delete;
using Application.Features.Ingredient.Get;
using Application.Features.Ingredient.GetById;
using Application.Features.Ingredient.NutritionSearch;
using Application.Features.Ingredient.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class IngredientsController : Controller
{
    private readonly IMediator _mediator;

    public IngredientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(
        int page = PaginationParameters.DefaultPage,
        int pageSize = PaginationParameters.DefaultPageSize,
        string? searchTerm = null)
    {
        var model = new IngredientIndexViewModel
        {
            SearchTerm = searchTerm
        };

        var response = await _mediator.Send(new GetIngredientsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        model.Ingredients = response.Adapt<AdminPanel.Models.Common.PaginatedViewModel<IngredientViewModel>>();

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var response = await _mediator.Send(new GetIngredientByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<IngredientViewModel>());
    }

    public IActionResult Create()
    {
        return View(new IngredientViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> NutritionSearch(string query, int pageSize = 10)
    {
        try
        {
            var response = await _mediator.Send(new SearchNutritionQuery
            {
                Query = query,
                PageSize = pageSize
            });

            return Json(response);
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            return BadRequest(MvcErrorHelper.GetDisplayMessage(exception));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(IngredientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = model.Adapt<CreateIngredientRequest>();
        if (!this.ValidateRequestDto(request)) return View(model);

        try
        {
            var command = request.Adapt<CreateIngredientCommand>();
            await _mediator.Send(command);
            MvcErrorHelper.SetSuccessMessage(TempData, "Ingredient created.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            MvcErrorHelper.AddToModelState(ModelState, exception);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var response = await _mediator.Send(new GetIngredientByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<IngredientViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, IngredientViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return View(model);

        var request = model.Adapt<UpdateIngredientRequest>();
        if (!this.ValidateRequestDto(request)) return View(model);

        try
        {
            var command = request.Adapt<UpdateIngredientCommand>();
            command.Id = id;
            await _mediator.Send(command);
            MvcErrorHelper.SetSuccessMessage(TempData, "Ingredient updated.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            MvcErrorHelper.AddToModelState(ModelState, exception);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _mediator.Send(new GetIngredientByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<IngredientViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        OperationResponse response = await _mediator.Send(new DeleteIngredientCommand { Id = id });
        if (!response.Success)
        {
            MvcErrorHelper.SetErrorMessage(TempData, response.Message);
            return RedirectToAction(nameof(Delete), new { id });
        }

        MvcErrorHelper.SetSuccessMessage(TempData, response.Message);
        return RedirectToAction(nameof(Index));
    }
}

