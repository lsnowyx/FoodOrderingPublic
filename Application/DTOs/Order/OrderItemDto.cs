namespace Application.DTOs.Order;

public sealed record OrderItemDto(Guid Id, Guid MenuItemId, string MenuItemName, int Quantity, decimal UnitPrice);
