using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Ingredient.Delete;

public sealed class DeleteIngredientCommand : IRequest<OperationResponse>
{
    public Guid Id { get; set; }
}
