using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItem;

public sealed record UpdateMenuItemRequest(
    [param: Required]
    [param: StringLength(100)]
    string Name,

    [param: StringLength(1000)]
    string? Description,

    [param: Range(0, 1_000_000)]
    decimal Price,

    Guid CategoryId,

    bool IsAvailable = true
);
