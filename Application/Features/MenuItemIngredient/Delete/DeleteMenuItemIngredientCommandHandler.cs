using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItemIngredient.Delete;

public class DeleteMenuItemIngredientCommandHandler : IRequestHandler<DeleteMenuItemIngredientCommand, OperationResponse>
{
    private readonly IMenuItemIngredientsRepository _repo;
    private readonly Application.Abstractions.Repositories.IMenuItemsRepository _menuRepo;

    public DeleteMenuItemIngredientCommandHandler(IMenuItemIngredientsRepository repo, Application.Abstractions.Repositories.IMenuItemsRepository menuRepo)
    {
        _repo = repo;
        _menuRepo = menuRepo;
    }

    public async Task<OperationResponse> Handle(DeleteMenuItemIngredientCommand request, CancellationToken cancellationToken)
    {
        var menu = await _menuRepo.GetByIdAsync(request.MenuItemId, cancellationToken);
        if (menu == null) throw new KeyNotFoundException("Menu item not found");

        var entity = await _repo.GetByMenuItemAndIngredientAsync(request.MenuItemId, request.IngredientId, cancellationToken);
        if (entity == null) throw new KeyNotFoundException("Menu item ingredient not found");

        await _repo.DeleteAsync(entity, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return new OperationResponse(true, "Menu item ingredient deleted.");
    }
}
