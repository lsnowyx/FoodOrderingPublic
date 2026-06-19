using Application.DTOs.Ingredient;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Ingredient.Get;

public sealed class GetIngredientsQuery : IRequest<PaginatedResponse<IngredientResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public string? SearchTerm { get; init; }
}
