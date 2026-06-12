using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.Get;

public sealed class GetIngredientsQuery : IRequest<IEnumerable<IngredientResponse>> { }
