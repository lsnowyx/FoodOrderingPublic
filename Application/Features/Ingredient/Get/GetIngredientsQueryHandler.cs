using Application.Abstractions.Repositories;
using Application.DTOs.Ingredient;
using Application.DTOs.Common;
using Mapster;
using MediatR;

namespace Application.Features.Ingredient.Get;

public class GetIngredientsQueryHandler : IRequestHandler<GetIngredientsQuery, PaginatedResponse<IngredientResponse>>
{
    private readonly IIngredientsRepository _repo;

    public GetIngredientsQueryHandler(IIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedResponse<IngredientResponse>> Handle(GetIngredientsQuery request, CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _repo.CountAsync(request.SearchTerm, cancellationToken);
        var ingredients = await _repo.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.SearchTerm,
            cancellationToken);
        var items = ingredients.Adapt<List<IngredientResponse>>();

        return PaginatedResponse<IngredientResponse>.Create(pagination, totalCount, items);
    }
}
