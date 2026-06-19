namespace Application.Abstractions.Services;

public interface IMenuItemCostService
{
    Task RecalculateMenuItemCostAsync(
        Guid menuItemId,
        CancellationToken cancellationToken = default);

    Task RecalculateMenuItemsUsingIngredientAsync(
        Guid ingredientId,
        CancellationToken cancellationToken = default);
}
