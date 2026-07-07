using Common.Constants;

namespace Application.Features.Order;

internal static class OrderItemQuantityGuard
{
    public static void EnsureValidLineQuantity(int quantity)
    {
        if (quantity is < 1 or > OrderValidationConstants.MaxItemQuantity)
        {
            throw new ArgumentException(
                $"Order item quantity must be between 1 and {OrderValidationConstants.MaxItemQuantity}.");
        }
    }

    public static void EnsureValidOrderShape<TItem>(
        IReadOnlyCollection<TItem>? items,
        Func<TItem, int> quantitySelector)
    {
        if (items is null || items.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one item line.");
        }

        if (items.Count > OrderValidationConstants.MaxOrderLineCount)
        {
            throw new ArgumentException(
                $"An order can contain at most {OrderValidationConstants.MaxOrderLineCount} item lines.");
        }

        long totalQuantity = 0;
        foreach (var item in items)
        {
            var quantity = quantitySelector(item);
            EnsureValidLineQuantity(quantity);

            totalQuantity += quantity;
        }

        if (totalQuantity > OrderValidationConstants.MaxTotalItemQuantity)
        {
            throw new ArgumentException(
                $"An order can contain at most {OrderValidationConstants.MaxTotalItemQuantity} items total.");
        }
    }
}
