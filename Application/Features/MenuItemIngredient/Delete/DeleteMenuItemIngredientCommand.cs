using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItemIngredient.Delete;

public sealed class DeleteMenuItemIngredientCommand : IRequest<OperationResponse>
{
    public Guid MenuItemId { get; set; }
    public Guid IngredientId { get; set; }
}
