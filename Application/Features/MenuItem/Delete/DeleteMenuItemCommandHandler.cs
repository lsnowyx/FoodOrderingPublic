using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.MenuItem.Delete;

public class DeleteMenuItemCommandHandler : IRequestHandler<DeleteMenuItemCommand, OperationResponse>
{
    private readonly IMenuItemsRepository _repo;

    public DeleteMenuItemCommandHandler(IMenuItemsRepository repo)
    {
        _repo = repo;
    }

    public async Task<OperationResponse> Handle(DeleteMenuItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Menu item not found");

        await _repo.DeleteAsync(request.Id, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return new OperationResponse(true, "Menu item deleted.");
    }
}
