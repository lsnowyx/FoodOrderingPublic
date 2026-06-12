using Application.DTOs.MenuItemPicture;
using MediatR;

namespace Application.Features.MenuItemPicture.Update;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public sealed class UpdateMenuItemPictureCommand : IRequest<MenuItemPictureResponse>
{
    public Guid MenuItemId { get; set; }
    public Guid PictureId { get; set; }

    [Required]
    public IFormFile ImageFile { get; set; } = null!;

    [StringLength(250)]
    public string? Caption { get; set; }
}
