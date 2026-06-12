using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.MenuItemPicture;

public sealed record MenuItemPictureRequest(
    [param:Required]
    IFormFile ImageFile,
    [param: StringLength(250)]
    string? Caption
);