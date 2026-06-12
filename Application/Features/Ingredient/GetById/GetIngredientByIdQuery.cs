using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.GetById;

public sealed class GetIngredientByIdQuery : IRequest<IngredientResponse?>
{
    public Guid Id { get; set; }
}
