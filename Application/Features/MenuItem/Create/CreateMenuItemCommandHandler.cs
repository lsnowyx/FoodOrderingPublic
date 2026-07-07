using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.MenuItem;
using Mapster;
using MediatR;

namespace Application.Features.MenuItem.Create;

public class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, MenuItemResponse>
{
    private readonly IMenuItemsRepository _repo;
    private readonly Application.Abstractions.Repositories.ICategoriesRepository _categoryRepo;
    private readonly ICacheService _cacheService;

    public CreateMenuItemCommandHandler(
        IMenuItemsRepository repo,
        Application.Abstractions.Repositories.ICategoriesRepository categoryRepo,
        ICacheService cacheService)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _cacheService = cacheService;
    }

    public async Task<MenuItemResponse> Handle(CreateMenuItemCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var category = await _categoryRepo.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) throw new KeyNotFoundException("Category not found");


        var toAdd = request.Adapt<Domain.Entities.MenuItem>();
        toAdd.Category = category;
        await _repo.AddAsync(toAdd, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        await CacheInvalidationHelper.InvalidateMenuItemCachesAsync(
            _cacheService,
            cancellationToken);
        return toAdd.Adapt<MenuItemResponse>();
    }
}
