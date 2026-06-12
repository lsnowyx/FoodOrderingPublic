using Application.Abstractions.Repositories;
using Application.DTOs.Category;
using Mapster;
using MediatR;

namespace Application.Features.Category.Get;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IEnumerable<CategoryResponse>>
{
    private readonly ICategoriesRepository _repo;

    public GetCategoriesQueryHandler(ICategoriesRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var all = await _repo.GetAllAsync(cancellationToken);
        return all.Select(c => c.Adapt<CategoryResponse>());
    }
}
