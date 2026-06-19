using Application.DTOs.Category;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Category.Lookup;

public sealed class GetCategoryLookupQuery : IRequest<PaginatedResponse<CategoryLookupResponse>>
{
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.LookupPageSize;
}
