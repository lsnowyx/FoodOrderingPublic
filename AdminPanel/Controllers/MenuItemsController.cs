using AdminPanel.Models.MenuItem;
using AdminPanel.Models.Category;
using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize]
public class MenuItemsController : Controller
{
    private readonly IApiClient _apiClient;

    public MenuItemsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var items = await _apiClient.GetAsync<IEnumerable<MenuItemViewModel>>("api/menu-items");
            var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
            var catDict = categories?.ToDictionary(c => c.Id, c => c.Name) ?? new Dictionary<Guid, string>();
            var list = (items ?? Enumerable.Empty<MenuItemViewModel>()).Select(i => {
                i.CategoryName = i.CategoryId != Guid.Empty && catDict.TryGetValue(i.CategoryId, out var n) ? n : null;
                return i;
            });
            return View(list);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "MenuItems") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return View(Enumerable.Empty<MenuItemViewModel>());
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<MenuItemViewModel>($"api/menu-items/{id}");
            if (item == null) return NotFound();
            var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
            item.CategoryName = categories?.FirstOrDefault(c => c.Id == item.CategoryId)?.Name;

            var vm = new MenuItemDetailsViewModel { Item = item };
            vm.Pictures = item.Pictures ?? Enumerable.Empty<MenuItemPictureViewModel>();

            // try to load assigned ingredients and all ingredients for adding
            try
            {
                var assigned = await _apiClient.GetAsync<IEnumerable<MenuItemIngredientViewModel>>($"api/menu-items/{id}/ingredients");
                vm.Ingredients = assigned ?? Enumerable.Empty<MenuItemIngredientViewModel>();

                var allIngredients = await _apiClient.GetAsync<IEnumerable<AdminPanel.Models.Ingredient.IngredientViewModel>>("api/ingredients");
                vm.AllIngredients = allIngredients ?? Enumerable.Empty<AdminPanel.Models.Ingredient.IngredientViewModel>();
            }
            catch (ForbiddenException)
            {
                // user not authorized to manage ingredients; leave lists empty
            }

            return View(vm);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> AddIngredient(Guid id, Guid ingredientId, string quantity)
    {
        if (ingredientId == Guid.Empty || string.IsNullOrWhiteSpace(quantity))
        {
            TempData["Error"] = "Ingredient and quantity are required.";
            return RedirectToAction("Details", new { id });
        }

        try
        {
            var payload = new { ingredientId = ingredientId, quantity = quantity };
            var created = await _apiClient.PostAsync<object, MenuItemIngredientViewModel>($"api/menu-items/{id}/ingredients", payload);
            return RedirectToAction("Details", new { id });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> UpdateIngredient(Guid id, Guid ingredientId, string quantity)
    {
        if (ingredientId == Guid.Empty || string.IsNullOrWhiteSpace(quantity))
        {
            TempData["Error"] = "Quantity is required.";
            return RedirectToAction("Details", new { id });
        }

        try
        {
            var payload = new { quantity = quantity };
            await _apiClient.PutAsync<object, object>($"api/menu-items/{id}/ingredients/{ingredientId}", payload);
            return RedirectToAction("Details", new { id });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> DeleteIngredient(Guid id, Guid ingredientId)
    {
        if (ingredientId == Guid.Empty)
        {
            TempData["Error"] = "Invalid ingredient.";
            return RedirectToAction("Details", new { id });
        }

        try
        {
            await _apiClient.DeleteAsync($"api/menu-items/{id}/ingredients/{ingredientId}");
            return RedirectToAction("Details", new { id });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> AddPicture(Guid id, MenuItemPictureViewModel model)
    {
        if (model.ImageFile == null)
        {
            TempData["Error"] = "Image file is required.";
            return RedirectToAction("Details", new { id });
        }

        try
        {
            var created = await _apiClient.UploadMenuItemPictureAsync(id, model.ImageFile, model.Caption);
            TempData["Success"] = "Picture added.";
            return RedirectToAction("Details", new { id });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> EditPicture(Guid id, Guid pictureId)
    {
        try
        {
            var pics = await _apiClient.GetAsync<IEnumerable<MenuItemPictureViewModel>>($"api/menu-items/{id}/pictures");
            var pic = (pics ?? Enumerable.Empty<MenuItemPictureViewModel>()).FirstOrDefault(p => p.Id == pictureId);
            if (pic == null) return NotFound();
            return View(pic);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> EditPicture(Guid id, Guid pictureId, MenuItemPictureViewModel model)
    {
        model.MenuItemId = id;
        model.Id = pictureId;
        if (!ModelState.IsValid) return View(model);
        if (model.ImageFile == null)
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Image file is required when replacing a picture.");
            return View(model);
        }

        try
        {
            await _apiClient.UpdateMenuItemPictureAsync(id, pictureId, model.ImageFile, model.Caption);
            TempData["Success"] = "Picture updated.";
            return RedirectToAction("Details", new { id });
        }
        catch (ApiException ex)
        {
            ApiErrorHelper.AddErrorsToModelState(ModelState, ex.Content);
            return View(model);
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> DeletePicture(Guid id, Guid pictureId)
    {
        try
        {
            await _apiClient.DeleteAsync($"api/menu-items/{id}/pictures/{pictureId}");
            TempData["Success"] = "Picture deleted.";
            return RedirectToAction("Details", new { id });
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return RedirectToAction("Details", new { id });
        }
    }

    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Create()
    {
        var vm = new MenuItemViewModel { IsAvailable = true };
        var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
        ViewBag.Categories = categories ?? Enumerable.Empty<CategoryViewModel>();
        return View(vm);
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Create(MenuItemViewModel model)
    {
        var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
        ViewBag.Categories = categories ?? Enumerable.Empty<CategoryViewModel>();
        if (!ModelState.IsValid) return View(model);
        try
        {
            var payload = new
            {
                name = model.Name,
                description = model.Description,
                price = model.Price,
                categoryId = model.CategoryId,
                isAvailable = model.IsAvailable
            };
            var created = await _apiClient.PostAsync<object, MenuItemViewModel>("api/menu-items", payload);
            if (created == null) return RedirectToAction("Index");

            TempData["Success"] = "Menu item created. Add pictures from this page.";
            return RedirectToAction("Details", new { id = created.Id });
        }
        catch (ApiException ex)
        {
            ApiErrorHelper.AddErrorsToModelState(ModelState, ex.Content);
            return View(model);
        }
    }

    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<MenuItemViewModel>($"api/menu-items/{id}");
            if (item == null) return NotFound();
            var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
            ViewBag.Categories = categories ?? Enumerable.Empty<CategoryViewModel>();
            return View(item);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Edit(Guid id, MenuItemViewModel model)
    {
        var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
        ViewBag.Categories = categories ?? Enumerable.Empty<CategoryViewModel>();
        if (!ModelState.IsValid) return View(model);
        try
        {
            var payload = new
            {
                name = model.Name,
                description = model.Description,
                price = model.Price,
                categoryId = model.CategoryId,
                isAvailable = model.IsAvailable
            };
            var updated = await _apiClient.PutAsync<object, MenuItemViewModel>($"api/menu-items/{id}", payload);
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            ApiErrorHelper.AddErrorsToModelState(ModelState, ex.Content);
            return View(model);
        }
    }

    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<MenuItemViewModel>($"api/menu-items/{id}");
            if (item == null) return NotFound();
            var categories = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
            item.CategoryName = categories?.FirstOrDefault(c => c.Id == item.CategoryId)?.Name;
            return View(item);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await _apiClient.DeleteAsync($"api/menu-items/{id}");
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? "Failed to delete menu item.";
            return RedirectToAction("Delete", new { id });
        }
    }
}
