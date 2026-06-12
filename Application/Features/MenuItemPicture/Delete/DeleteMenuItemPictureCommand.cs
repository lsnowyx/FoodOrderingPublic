using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItemPicture.Delete;

public sealed class DeleteMenuItemPictureCommand : IRequest<OperationResponse>
{
    public Guid MenuItemId { get; set; }
    public Guid PictureId { get; set; }
}
