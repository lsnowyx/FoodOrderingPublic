using System.Globalization;

namespace Application.Caching;

public static class CacheKeyHelper
{
    public const string NominatimSearchPrefix = "geo:nominatim:search";
    public const string NutritionSearchPrefix = "nutrition:fdc:search";
    public const string CatalogCategoriesPrefix = "catalog:categories";
    public const string CatalogMenuItemsPrefix = "catalog:menu-items";
    public const string AdminCategoriesPrefix = "admin:categories";
    public const string AdminIngredientsPrefix = "admin:ingredients";
    public const string AdminMenuItemsPrefix = "admin:menu-items";
    public const string CategoryLookupPrefix = "lookup:categories";
    public const string IngredientLookupPrefix = "lookup:ingredients";

    public static string NominatimSearch(
        string query,
        int limit,
        string? countryCodes)
    {
        return Join(
            NominatimSearchPrefix,
            Pair("q", NormalizeSearchValue(query)),
            Pair("limit", limit.ToString(CultureInfo.InvariantCulture)),
            Pair("countrycodes", NormalizeSearchValue(countryCodes)));
    }

    public static string NutritionSearch(string query, int pageSize)
    {
        return Join(
            NutritionSearchPrefix,
            Pair("q", NormalizeSearchValue(query)),
            Pair("pageSize", pageSize.ToString(CultureInfo.InvariantCulture)));
    }

    public static string CatalogCategories(int page, int pageSize)
    {
        return Join(CatalogCategoriesPrefix, PagePart(page), PageSizePart(pageSize));
    }

    public static string CatalogMenuItemsByCategory(
        Guid categoryId,
        int page,
        int pageSize)
    {
        return Join(
            CatalogMenuItemsPrefix,
            Pair("categoryId", categoryId.ToString("N")),
            PagePart(page),
            PageSizePart(pageSize));
    }

    public static string AdminCategories(
        int page,
        int pageSize,
        string? searchTerm)
    {
        return Join(
            AdminCategoriesPrefix,
            PagePart(page),
            PageSizePart(pageSize),
            Pair("search", NormalizeSearchValue(searchTerm)));
    }

    public static string AdminIngredients(
        int page,
        int pageSize,
        string? searchTerm)
    {
        return Join(
            AdminIngredientsPrefix,
            PagePart(page),
            PageSizePart(pageSize),
            Pair("search", NormalizeSearchValue(searchTerm)));
    }

    public static string AdminMenuItems(
        int page,
        int pageSize,
        string? searchTerm,
        Guid? categoryId,
        bool? isAvailable)
    {
        return Join(
            AdminMenuItemsPrefix,
            PagePart(page),
            PageSizePart(pageSize),
            Pair("search", NormalizeSearchValue(searchTerm)),
            Pair("categoryId", categoryId?.ToString("N") ?? "all"),
            Pair("isAvailable", isAvailable?.ToString().ToLowerInvariant() ?? "all"));
    }

    public static string CategoryLookup(
        string? searchTerm,
        int page,
        int pageSize)
    {
        return Join(
            CategoryLookupPrefix,
            PagePart(page),
            PageSizePart(pageSize),
            Pair("search", NormalizeSearchValue(searchTerm)));
    }

    public static string IngredientLookup(
        string? searchTerm,
        int page,
        int pageSize)
    {
        return Join(
            IngredientLookupPrefix,
            PagePart(page),
            PageSizePart(pageSize),
            Pair("search", NormalizeSearchValue(searchTerm)));
    }

    public static string NormalizeSearchValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "all";
        }

        var normalized = CollapseWhitespace(value.Trim().ToLowerInvariant());

        return string.IsNullOrWhiteSpace(normalized)
            ? "all"
            : Escape(normalized);
    }

    private static string PagePart(int page)
    {
        return Pair("page", Math.Max(page, 1).ToString(CultureInfo.InvariantCulture));
    }

    private static string PageSizePart(int pageSize)
    {
        return Pair("pageSize", Math.Max(pageSize, 1).ToString(CultureInfo.InvariantCulture));
    }

    private static string Pair(string name, string value)
    {
        return $"{name}={value}";
    }

    private static string Join(params string[] parts)
    {
        return string.Join(':', parts);
    }

    private static string Escape(string value)
    {
        return Uri.EscapeDataString(value);
    }

    private static string CollapseWhitespace(string value)
    {
        var parts = new List<string>();
        var current = new List<char>();

        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (current.Count > 0)
                {
                    parts.Add(new string(current.ToArray()));
                    current.Clear();
                }

                continue;
            }

            current.Add(character);
        }

        if (current.Count > 0)
        {
            parts.Add(new string(current.ToArray()));
        }

        return string.Join(' ', parts);
    }
}
