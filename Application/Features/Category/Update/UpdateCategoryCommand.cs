using Application.DTOs.Category;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Category.Update;

using System.ComponentModel.DataAnnotations;

public sealed class UpdateCategoryCommand : IRequest<CategoryResponse>
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public IFormFile? ImageFile { get; set; }
}
