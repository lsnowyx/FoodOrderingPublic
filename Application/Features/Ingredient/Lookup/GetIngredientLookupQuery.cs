using Application.DTOs.Common;
using Application.DTOs.Ingredient;
using MediatR;

namespace Application.Features.Ingredient.Lookup;

public sealed class GetIngredientLookupQuery : IRequest<PaginatedResponse<IngredientLookupResponse>>
{
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.LookupPageSize;
}
