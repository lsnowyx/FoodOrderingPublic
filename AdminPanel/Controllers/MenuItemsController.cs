using AdminPanel.Models.Category;
using AdminPanel.Models.Common;
using AdminPanel.Models.Lookup;
using AdminPanel.Models.MenuItem;
using AdminPanel.Services;
using Application.DTOs.Category;
using Application.DTOs.Common;
using Application.DTOs.Ingredient;
using Application.DTOs.MenuItem;
using Application.DTOs.MenuItemIngredient;
using Application.DTOs.MenuItemPicture;
using Application.Features.Category.GetById;
using Application.Features.Category.Lookup;
using Application.Features.Ingredient.Lookup;
using Application.Features.MenuItem.Create;
using Application.Features.MenuItem.Delete;
using Application.Features.MenuItem.Get;
using Application.Features.MenuItem.GetById;
using Application.Features.MenuItem.Update;
using Application.Features.MenuItemIngredient.Create;
using Application.Features.MenuItemIngredient.Delete;
using Application.Features.MenuItemIngredient.Update;
using Application.Features.MenuItemPicture.Create;
using Application.Features.MenuItemPicture.Delete;
using Application.Features.MenuItemPicture.Update;
using Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize(AuthorizationPolicyConstants.MENU_MANAGER_POLICY)]
public class MenuItemsController : Controller
{
    private readonly IMediator _mediator;

    public MenuItemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<IActionResult> Index(
        int page = PaginationParameters.DefaultPage,
        int pageSize = PaginationParameters.DefaultPageSize,
        string? searchTerm = null,
        Guid? categoryId = null,
        bool? isAvailable = null)
    {
        var categories = await GetCategoryFilterOptionsAsync(categoryId);
        var response = await _mediator.Send(new GetMenuItemsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IsAvailable = isAvailable
        });

        var model = new MenuItemIndexViewModel
        {
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IsAvailable = isAvailable,
            Categories = categories,
            MenuItems = response.Adapt<PaginatedViewModel<MenuItemViewModel>>()
        };

        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var response = await _mediator.Send(new GetMenuItemByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<MenuItemDetailsViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> AddIngredient(
        Guid id,
        Guid ingredientId,
        decimal? quantity)
    {
        var request = new CreateMenuItemIngredientRequest(ingredientId, quantity);
        if (!this.ValidateRequestDtoForRedirect(request, "Ingredient and quantity are required."))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var command = request.Adapt<CreateMenuItemIngredientCommand>();
        command.MenuItemId = id;

        await _mediator.Send(command);
        MvcErrorHelper.SetSuccessMessage(TempData, "Ingredient added.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateIngredient(
        Guid id,
        Guid ingredientId,
        decimal? quantity)
    {
        var request = new UpdateMenuItemIngredientRequest(quantity);
        if (!this.ValidateRequestDtoForRedirect(request, "Quantity is required."))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var command = request.Adapt<UpdateMenuItemIngredientCommand>();
        command.MenuItemId = id;
        command.IngredientId = ingredientId;

        await _mediator.Send(command);
        MvcErrorHelper.SetSuccessMessage(TempData, "Ingredient quantity updated.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteIngredient(Guid id, Guid ingredientId)
    {
        var response = await _mediator.Send(new DeleteMenuItemIngredientCommand
        {
            MenuItemId = id,
            IngredientId = ingredientId
        });

        if (response.Success)
        {
            MvcErrorHelper.SetSuccessMessage(TempData, response.Message);
        }
        else
        {
            MvcErrorHelper.SetErrorMessage(TempData, response.Message);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AddPicture(Guid id, MenuItemPictureViewModel model)
    {
        var request = model.Adapt<MenuItemPictureRequest>();
        if (!this.ValidateRequestDtoForRedirect(request, "Image file is required."))
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var command = request.Adapt<CreateMenuItemPictureCommand>();
        command.MenuItemId = id;

        await _mediator.Send(command);
        MvcErrorHelper.SetSuccessMessage(TempData, "Picture added.");
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> EditPicture(Guid id, Guid pictureId)
    {
        var menuItem = await _mediator.Send(new GetMenuItemByIdQuery { Id = id });
        if (menuItem is null) return NotFound();

        var picture = menuItem.Pictures.FirstOrDefault(item => item.Id == pictureId);
        if (picture is null) return NotFound();

        return View(picture.Adapt<MenuItemPictureViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> EditPicture(Guid id, Guid pictureId, MenuItemPictureViewModel model)
    {
        model.MenuItemId = id;
        model.Id = pictureId;

        if (!ModelState.IsValid) return View(model);

        var request = model.Adapt<MenuItemPictureRequest>();
        if (!this.ValidateRequestDto(request)) return View(model);

        try
        {
            var command = request.Adapt<UpdateMenuItemPictureCommand>();
            command.MenuItemId = id;
            command.PictureId = pictureId;

            await _mediator.Send(command);
            MvcErrorHelper.SetSuccessMessage(TempData, "Picture updated.");
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            MvcErrorHelper.AddToModelState(ModelState, exception);
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeletePicture(Guid id, Guid pictureId)
    {
        var response = await _mediator.Send(new DeleteMenuItemPictureCommand
        {
            MenuItemId = id,
            PictureId = pictureId
        });

        if (response.Success)
        {
            MvcErrorHelper.SetSuccessMessage(TempData, response.Message);
        }
        else
        {
            MvcErrorHelper.SetErrorMessage(TempData, response.Message);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public IActionResult Create()
    {
        return View(new MenuItemViewModel { IsAvailable = true });
    }

    [HttpPost]
    public async Task<IActionResult> Create(MenuItemViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await ApplyCategoryNameAsync(model);
            return View(model);
        }

        var request = model.Adapt<CreateMenuItemRequest>();
        if (!this.ValidateRequestDto(request))
        {
            await ApplyCategoryNameAsync(model);
            return View(model);
        }

        try
        {
            var command = request.Adapt<CreateMenuItemCommand>();
            var response = await _mediator.Send(command);

            MvcErrorHelper.SetSuccessMessage(
                TempData,
                "Menu item created. Add pictures from this page.");
            return RedirectToAction(nameof(Details), new { id = response.Id });
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            MvcErrorHelper.AddToModelState(ModelState, exception);
            await ApplyCategoryNameAsync(model);
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var response = await _mediator.Send(new GetMenuItemByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<MenuItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, MenuItemViewModel model)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            await ApplyCategoryNameAsync(model);
            return View(model);
        }

        var request = model.Adapt<UpdateMenuItemRequest>();
        if (!this.ValidateRequestDto(request))
        {
            await ApplyCategoryNameAsync(model);
            return View(model);
        }

        try
        {
            var command = request.Adapt<UpdateMenuItemCommand>();
            command.Id = id;

            await _mediator.Send(command);
            MvcErrorHelper.SetSuccessMessage(TempData, "Menu item updated.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception) when (MvcErrorHelper.IsFormBusinessException(exception))
        {
            MvcErrorHelper.AddToModelState(ModelState, exception);
            await ApplyCategoryNameAsync(model);
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _mediator.Send(new GetMenuItemByIdQuery { Id = id });
        if (response is null) return NotFound();

        return View(response.Adapt<MenuItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        OperationResponse response = await _mediator.Send(new DeleteMenuItemCommand { Id = id });
        if (!response.Success)
        {
            MvcErrorHelper.SetErrorMessage(TempData, response.Message);
            return RedirectToAction(nameof(Delete), new { id });
        }

        MvcErrorHelper.SetSuccessMessage(TempData, response.Message);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CategoryLookup(
        string? searchTerm = null,
        int page = PaginationParameters.DefaultPage)
    {
        var response = await _mediator.Send(new GetCategoryLookupQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = PaginationParameters.LookupPageSize
        });

        return Json(ToSelect2Response(response));
    }

    [HttpGet]
    public async Task<IActionResult> IngredientLookup(
        string? searchTerm = null,
        int page = PaginationParameters.DefaultPage)
    {
        var response = await _mediator.Send(new GetIngredientLookupQuery
        {
            SearchTerm = searchTerm,
            Page = page,
            PageSize = PaginationParameters.LookupPageSize
        });

        return Json(ToSelect2Response(response));
    }

    private async Task<IReadOnlyList<CategoryViewModel>> GetCategoryFilterOptionsAsync(Guid? categoryId)
    {
        if (!categoryId.HasValue)
        {
            return Array.Empty<CategoryViewModel>();
        }

        var category = await _mediator.Send(new GetCategoryByIdQuery { Id = categoryId.Value });
        return category is null
            ? Array.Empty<CategoryViewModel>()
            : new[] { category.Adapt<CategoryViewModel>() };
    }

    private async Task ApplyCategoryNameAsync(MenuItemViewModel menuItem)
    {
        if (menuItem.CategoryId == Guid.Empty)
        {
            return;
        }

        var category = await _mediator.Send(new GetCategoryByIdQuery { Id = menuItem.CategoryId });
        menuItem.CategoryName = category?.Name;
    }

    private static Select2LookupResponseViewModel ToSelect2Response(
        PaginatedResponse<CategoryLookupResponse> categories)
    {
        return new Select2LookupResponseViewModel
        {
            Results = categories.Items
                .Select(category => new Select2LookupItemViewModel
                {
                    Id = category.Id.ToString(),
                    Text = category.Text,
                    Description = category.Description
                })
                .ToList(),
            Pagination = new Select2PaginationViewModel
            {
                More = categories.Page < categories.TotalPages
            }
        };
    }

    private static Select2LookupResponseViewModel ToSelect2Response(
        PaginatedResponse<IngredientLookupResponse> ingredients)
    {
        return new Select2LookupResponseViewModel
        {
            Results = ingredients.Items
                .Select(ingredient => new Select2LookupItemViewModel
                {
                    Id = ingredient.Id.ToString(),
                    Text = ingredient.Text,
                    BaseUnit = ingredient.BaseUnit,
                    UnitCost = ingredient.UnitCost,
                    AllergenInfo = ingredient.AllergenInfo,
                    CaloriesPerUnit = ingredient.CaloriesPerUnit
                })
                .ToList(),
            Pagination = new Select2PaginationViewModel
            {
                More = ingredients.Page < ingredients.TotalPages
            }
        };
    }

}
