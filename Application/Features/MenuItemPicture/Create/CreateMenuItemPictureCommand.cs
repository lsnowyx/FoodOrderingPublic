using Application.DTOs.MenuItemPicture;
using MediatR;

namespace Application.Features.MenuItemPicture.Create;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public sealed class CreateMenuItemPictureCommand : IRequest<MenuItemPictureResponse>
{
    public Guid MenuItemId { get; set; }

    public IFormFile ImageFile { get; set; } = null!;

    [StringLength(250)]
    public string? Caption { get; set; }
}
