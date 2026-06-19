using AdminPanel.Models.Category;
using AdminPanel.Services;
using Application.DTOs.Category;
using Application.DTOs.Common;
using Application.Features.Category.Create;
using Application.Features.Category.Delete;
using Application.Features.Category.Get;
using Application.Features.Category.GetById;
using Application.Features.Category.Update;
using Application.Features.MenuItem.Get;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class CategoriesController : Controller
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(
        int page = PaginationParameters.DefaultPage,
        int pageSize = PaginationParameters.DefaultPageSize,
        string? searchTerm = null)
    {
        var model = new CategoryIndexViewModel
        {
            SearchTerm = searchTerm
        };

        var response = await _mediator.Send(new GetCategoriesQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm
        });

        model.Categories = response.Adapt<AdminPanel.Models.Common.PaginatedViewModel<CategoryViewModel>>();

        return View(model);
    }

    public async Task<IActionResult> Details(
        Guid id,
        int page = PaginationParameters.DefaultPage,
        int pageSize = PaginationParameters.DefaultPageSize)
    {
        var categoryResponse = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
        if (categoryResponse is null) return NotFound();

        var menuItemsResponse = await _mediator.Send(new GetMenuItemsQuery
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = id
        });

        var model = new CategoryDetailsViewModel
        {
            Category = categoryResponse.Adapt<CategoryViewModel>(),
            MenuItems = menuItemsResponse.Adapt<
                AdminPanel.Models.Common.PaginatedViewModel<
                    AdminPanel.Models.MenuItem.MenuItemViewModel>>()
        };

        return View(model);
    }

    public IActionResult Create()
    {
        return View(new CategoryViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var request = model.Adapt<CreateCategoryRequest>();
        if (!TryValidateModel(request)) return View(model);

        try
        {
            var command = request.Adapt<CreateCategoryCommand>();
            await _mediator.Send(command);
            MvcErrorHelper.SetSuccessMessage(TempData, "Category created.");
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
        var response = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<CategoryViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, CategoryViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return View(model);

        var request = model.Adapt<UpdateCategoryRequest>();
        if (!TryValidateModel(request)) return View(model);

        try
        {
            var command = request.Adapt<UpdateCategoryCommand>();
            command.Id = id;
            await _mediator.Send(command);
            MvcErrorHelper.SetSuccessMessage(TempData, "Category updated.");
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
        var response = await _mediator.Send(new GetCategoryByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<CategoryViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        OperationResponse response = await _mediator.Send(new DeleteCategoryCommand { Id = id });
        if (!response.Success)
        {
            MvcErrorHelper.SetErrorMessage(TempData, response.Message);
            return RedirectToAction(nameof(Delete), new { id });
        }

        MvcErrorHelper.SetSuccessMessage(TempData, response.Message);
        return RedirectToAction(nameof(Index));
    }
}

