namespace Application.DTOs.Order;

public sealed record CustomerOrderItemResponse(
    Guid MenuItemId,
    string MenuItemName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal TotalCalories);
