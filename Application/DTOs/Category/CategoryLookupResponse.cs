namespace Application.DTOs.Category;

public sealed record CategoryLookupResponse(Guid Id, string Text, string? Description);
