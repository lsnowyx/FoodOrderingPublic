using Application.Abstractions.Repositories;
using Application.DTOs.Category;
using Application.DTOs.Common;
using Mapster;
using MediatR;

namespace Application.Features.Category.Lookup;

public sealed class GetCategoryLookupQueryHandler
    : IRequestHandler<GetCategoryLookupQuery, PaginatedResponse<CategoryLookupResponse>>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public GetCategoryLookupQueryHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public async Task<PaginatedResponse<CategoryLookupResponse>> Handle(
        GetCategoryLookupQuery request,
        CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _categoriesRepository.CountAsync(request.SearchTerm, cancellationToken);
        var categories = await _categoriesRepository.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.SearchTerm,
            cancellationToken);
        var items = categories.Adapt<List<CategoryLookupResponse>>();

        return PaginatedResponse<CategoryLookupResponse>.Create(pagination, totalCount, items);
    }
}
