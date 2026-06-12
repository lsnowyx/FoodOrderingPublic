namespace Application.DTOs.MenuItem;

public sealed record MenuItemResponse(Guid Id, string Name, string? Description, decimal Price, Guid CategoryId, bool IsAvailable);
