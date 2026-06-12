namespace Application.DTOs.Category;

public sealed record CategoryResponse(Guid Id, string Name, string? Description, string? ImageUrl);
