using Domain.Entities;

namespace Application.Abstractions.Repositories;

public interface IMenuItemIngredientsRepository
{
    Task<IEnumerable<MenuItemIngredient>> GetByMenuItemIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
    Task<MenuItemIngredient?> GetByMenuItemAndIngredientAsync(Guid menuItemId, Guid ingredientId, CancellationToken cancellationToken = default);

    Task<MenuItemIngredient> AddAsync(MenuItemIngredient entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(MenuItemIngredient entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(MenuItemIngredient entity, CancellationToken cancellationToken = default);

    Task<bool> ExistsByMenuItemAndIngredientAsync(Guid menuItemId, Guid ingredientId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
