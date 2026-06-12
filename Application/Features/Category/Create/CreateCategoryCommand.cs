using Application.DTOs.Category;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Category.Create;

using System.ComponentModel.DataAnnotations;

public sealed class CreateCategoryCommand : IRequest<CategoryResponse>
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public IFormFile? ImageFile { get; set; }
}
