using AdminPanel.Models.Ingredient;
using AdminPanel.Services;
using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers;

[Authorize]
public class IngredientsController : Controller
{
    private readonly IApiClient _apiClient;

    public IngredientsController(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var list = await _apiClient.GetAsync<IEnumerable<IngredientViewModel>>("api/ingredients");
            return View(list ?? Enumerable.Empty<IngredientViewModel>());
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Ingredients") });
        }
        catch (ForbiddenException)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Content ?? ex.Message;
            return View(Enumerable.Empty<IngredientViewModel>());
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<IngredientViewModel>($"api/ingredients/{id}");
            if (item == null) return NotFound();
            return View(item);
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public IActionResult Create()
    {
        return View(new IngredientViewModel());
    }

    [HttpPost]
    [Authorize(Roles = UserRoleConstants.MENU_MANAGER_ROLE)]
    public async Task<IActionResult> Create(IngredientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var payload = new { name = model.Name, allergenInfo = model.AllergenInfo, caloriesPerUnit = model.CaloriesPerUnit };
            var created = await _apiClient.PostAsync<object, IngredientViewModel>("api/ingredients", payload);
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
            var item = await _apiClient.GetAsync<IngredientViewModel>($"api/ingredients/{id}");
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
    public async Task<IActionResult> Edit(Guid id, IngredientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var payload = new { name = model.Name, allergenInfo = model.AllergenInfo, caloriesPerUnit = model.CaloriesPerUnit };
            var updated = await _apiClient.PutAsync<object, IngredientViewModel>($"api/ingredients/{id}", payload);
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            ApiErrorHelper.AddErrorsToModelState(ModelState, ex.Content);
            return View(model);
        }
    }

    [Authorize(Roles = "MenuManager")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var item = await _apiClient.GetAsync<IngredientViewModel>($"api/ingredients/{id}");
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
            await _apiClient.DeleteAsync($"api/ingredients/{id}");
            return RedirectToAction("Index");
        }
        catch (ApiException ex)
        {
            // if delete fails because of FK constraints, show friendly message
            if (ex.Content != null && ex.Content.Contains("used by") || ex.Content.Contains("constraint", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Ingredient cannot be deleted because it is used by existing menu items.";
            }
            else
            {
                TempData["Error"] = ex.Content ?? "Failed to delete ingredient.";
            }
            return RedirectToAction("Delete", new { id });
        }
    }
}

