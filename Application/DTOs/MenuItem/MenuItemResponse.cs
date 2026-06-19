namespace Application.DTOs.MenuItem;

public sealed record MenuItemResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal RestaurantCost,
    decimal TotalCalories,
    Guid CategoryId,
    string CategoryName,
    bool IsAvailable);
