using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.MenuItem;
using Mapster;
using MediatR;

namespace Application.Features.MenuItem.Update;

public class UpdateMenuItemCommandHandler : IRequestHandler<UpdateMenuItemCommand, MenuItemResponse>
{
    private readonly IMenuItemsRepository _repo;
    private readonly Application.Abstractions.Repositories.ICategoriesRepository _categoryRepo;
    private readonly ICacheService _cacheService;

    public UpdateMenuItemCommandHandler(
        IMenuItemsRepository repo,
        Application.Abstractions.Repositories.ICategoriesRepository categoryRepo,
        ICacheService cacheService)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _cacheService = cacheService;
    }

    public async Task<MenuItemResponse> Handle(UpdateMenuItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Menu item not found");

        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) throw new KeyNotFoundException("Category not found");


        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Price = request.Price;
        existing.CategoryId = request.CategoryId;
        existing.Category = category;
        existing.IsAvailable = request.IsAvailable;

        await _repo.UpdateAsync(existing, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        await CacheInvalidationHelper.InvalidateMenuItemCachesAsync(
            _cacheService,
            cancellationToken);

        return existing.Adapt<MenuItemResponse>();
    }
}
