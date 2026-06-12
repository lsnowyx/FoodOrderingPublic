using Application.Abstractions.Repositories;
using Application.DTOs.Category;
using Mapster;
using MediatR;

namespace Application.Features.Category.GetById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryResponse?>
{
    private readonly ICategoriesRepository _repo;

    public GetCategoryByIdQueryHandler(ICategoriesRepository repo)
    {
        _repo = repo;
    }

    public async Task<CategoryResponse?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repo.GetByIdAsync(request.Id, cancellationToken);
        return entity?.Adapt<CategoryResponse>();
    }
}
