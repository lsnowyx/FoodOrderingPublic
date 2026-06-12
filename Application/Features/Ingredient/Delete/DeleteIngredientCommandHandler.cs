using Application.Abstractions.Repositories;
using Application.DTOs.Common;
using MediatR;

namespace Application.Features.Ingredient.Delete;

public class DeleteIngredientCommandHandler : IRequestHandler<DeleteIngredientCommand, OperationResponse>
{
    private readonly IIngredientsRepository _repo;

    public DeleteIngredientCommandHandler(IIngredientsRepository repo)
    {
        _repo = repo;
    }

    public async Task<OperationResponse> Handle(DeleteIngredientCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (existing == null) throw new KeyNotFoundException("Ingredient not found");

        var used = await _repo.IsUsedAsync(request.Id, cancellationToken);
        if (used) throw new InvalidOperationException("Ingredient is used by menu items and cannot be deleted.");

        await _repo.DeleteAsync(request.Id, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return new OperationResponse(true, "Ingredient deleted.");
    }
}
