using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using Application.DTOs.MenuItem;
using Mapster;
using MediatR;

namespace Application.Features.MenuItem.Get;

public class GetMenuItemsQueryHandler : IRequestHandler<GetMenuItemsQuery, PaginatedResponse<MenuItemResponse>>
{
    private readonly IMenuItemsRepository _repo;

    public GetMenuItemsQueryHandler(IMenuItemsRepository repo)
    {
        _repo = repo;
    }

    public async Task<PaginatedResponse<MenuItemResponse>> Handle(GetMenuItemsQuery request, CancellationToken cancellationToken)
    {
        var pagination = PaginationParameters.Create(request.Page, request.PageSize);

        var totalCount = await _repo.CountAsync(
            request.SearchTerm,
            request.CategoryId,
            request.IsAvailable,
            cancellationToken);
        var menuItems = await _repo.GetPagedAsync(
            pagination.Skip,
            pagination.PageSize,
            request.SearchTerm,
            request.CategoryId,
            request.IsAvailable,
            cancellationToken);
        var items = menuItems.Adapt<List<MenuItemResponse>>();

        return PaginatedResponse<MenuItemResponse>.Create(pagination, totalCount, items);
    }
}
