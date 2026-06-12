using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Category;

public sealed record UpdateCategoryRequest(
    [param: Required]
    [param: StringLength(100)]
    string Name,

    [param: StringLength(500)]
    string? Description,

    IFormFile? ImageFile
);
