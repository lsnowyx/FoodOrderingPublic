using Application.Abstractions.Repositories;
using Application.DTOs.Category;
using Application.DTOs.Common;
using Mapster;
using MediatR;

namespace Application.Features.Category.Get;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, PaginatedResponse<CategoryResponse>>
{
    private readonly ICategoriesRepository _repo;

    public GetCategoriesQueryHandler(ICategoriesRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedResponse<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _repo.CountAsync(request.SearchTerm, cancellationToken);
        var categories = await _repo.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.SearchTerm,
            cancellationToken);
        var items = categories.Adapt<List<CategoryResponse>>();

        return PaginatedResponse<CategoryResponse>.Create(pagination, totalCount, items);
    }
}
