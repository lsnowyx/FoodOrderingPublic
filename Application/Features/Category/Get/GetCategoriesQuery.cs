using Application.DTOs.Category;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Category.Get;

public sealed class GetCategoriesQuery : IRequest<PaginatedResponse<CategoryResponse>>
{
    public int Page { get; init; } = PaginationParameters.DefaultPage;
    public int PageSize { get; init; } = PaginationParameters.DefaultPageSize;
    public string? SearchTerm { get; init; }
}
