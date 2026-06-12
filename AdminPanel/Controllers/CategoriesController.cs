using AdminPanel.Models.Category;
using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly IApiClient _apiClient;

    public CategoriesController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await _apiClient.GetAsync<IEnumerable<CategoryViewModel>>("api/categories");
            return View(list ?? Enumerable.Empty<CategoryViewModel>());
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Categories") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return View(Enumerable.Empty<CategoryViewModel>());
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<CategoryViewModel>($"api/categories/{id}");
            if (item == null) return NotFound();

            // fetch menu items and filter by category
            IEnumerable<AdminPanel.Models.MenuItem.MenuItemViewModel> menuItems = Enumerable.Empty<AdminPanel.Models.MenuItem.MenuItemViewModel>();
            try
            {
                var all = await _apiClient.GetAsync<IEnumerable<AdminPanel.Models.MenuItem.MenuItemViewModel>>("api/menu-items");
                menuItems = (all ?? Enumerable.Empty<AdminPanel.Models.MenuItem.MenuItemViewModel>()).Where(m => m.CategoryId == id);
            }
            catch (ForbiddenException)
            {
                // ignore; user may not have permission to view menu management
                menuItems = Enumerable.Empty<AdminPanel.Models.MenuItem.MenuItemViewModel>();
            }

            var vm = new CategoryDetailsViewModel { Category = item, MenuItems = menuItems };
            return View(vm);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public IActionResult Create()
    {
        return View(new CategoryViewModel());
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var created = await _apiClient.CreateCategoryAsync(model);
            return RedirectToAction("Index");
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
            var item = await _apiClient.GetAsync<CategoryViewModel>($"api/categories/{id}");
            if (item == null) return NotFound();
            return View(item);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Edit(Guid id, CategoryViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return View(model);
        try
        {
            var updated = await _apiClient.UpdateCategoryAsync(id, model);
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
            var item = await _apiClient.GetAsync<CategoryViewModel>($"api/categories/{id}");
            if (item == null) return NotFound();
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
            await _apiClient.DeleteAsync($"api/categories/{id}");
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? "Failed to delete category.";
            return RedirectToAction("Delete", new { id });
        }
    }
}

