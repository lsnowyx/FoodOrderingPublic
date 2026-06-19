using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using Application.DTOs.Ingredient;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Lookup;

public sealed class GetIngredientLookupQueryHandler
    : IRequestHandler<GetIngredientLookupQuery, PaginatedResponse<IngredientLookupResponse>>
{
    private readonly IIngredientsRepository _ingredientsRepository;

    public GetIngredientLookupQueryHandler(IIngredientsRepository ingredientsRepository)
    {
        _ingredientsRepository = ingredientsRepository;
    }

    public async Task<PaginatedResponse<IngredientLookupResponse>> Handle(
        GetIngredientLookupQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _ingredientsRepository.CountAsync(request.SearchTerm, cancellationToken);
        var ingredients = await _ingredientsRepository.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.SearchTerm,
            cancellationToken);
        var items = ingredients.Adapt<List<IngredientLookupResponse>>();

        return PaginatedResponse<IngredientLookupResponse>.Create(pagination, totalCount, items);
    }
}
