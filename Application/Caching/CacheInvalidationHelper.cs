using Application.Abstractions.Services;

namespace Application.Caching;

public static class CacheInvalidationHelper
{
    public static Task InvalidateCategoryCachesAsync(
        ICacheService cacheService,
        CancellationToken cancellationToken)
    {
        return RemovePrefixesAsync(
            cacheService,
            cancellationToken,
            CacheKeyHelper.CatalogCategoriesPrefix,
            CacheKeyHelper.AdminCategoriesPrefix,
            CacheKeyHelper.CategoryLookupPrefix,
            CacheKeyHelper.CatalogMenuItemsPrefix,
            CacheKeyHelper.AdminMenuItemsPrefix);
    }

    public static Task InvalidateMenuItemCachesAsync(
        ICacheService cacheService,
        CancellationToken cancellationToken)
    {
        return RemovePrefixesAsync(
            cacheService,
            cancellationToken,
            CacheKeyHelper.CatalogMenuItemsPrefix,
            CacheKeyHelper.AdminMenuItemsPrefix);
    }

    public static Task InvalidateIngredientCachesAsync(
        ICacheService cacheService,
        CancellationToken cancellationToken)
    {
        return RemovePrefixesAsync(
            cacheService,
            cancellationToken,
            CacheKeyHelper.AdminIngredientsPrefix,
            CacheKeyHelper.IngredientLookupPrefix,
            CacheKeyHelper.AdminMenuItemsPrefix,
            CacheKeyHelper.CatalogMenuItemsPrefix);
    }

    private static Task RemovePrefixesAsync(
        ICacheService cacheService,
        CancellationToken cancellationToken,
        params string[] prefixes)
    {
        return Task.WhenAll(prefixes.Select(prefix =>
            cacheService.RemoveByPrefixAsync(prefix, cancellationToken)));
    }
}
