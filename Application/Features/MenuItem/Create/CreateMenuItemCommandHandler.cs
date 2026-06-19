using Application.Abstractions.Repositories;
using Application.DTOs.MenuItem;
using Mapster;
using MediatR;

namespace Application.Features.MenuItem.Create;

public class CreateMenuItemCommandHandler : IRequestHandler<CreateMenuItemCommand, MenuItemResponse>
{
    private readonly IMenuItemsRepository _repo;
    private readonly Application.Abstractions.Repositories.ICategoriesRepository _categoryRepo;

    public CreateMenuItemCommandHandler(IMenuItemsRepository repo, Application.Abstractions.Repositories.ICategoriesRepository categoryRepo)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
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
        return toAdd.Adapt<MenuItemResponse>();
    }
}
