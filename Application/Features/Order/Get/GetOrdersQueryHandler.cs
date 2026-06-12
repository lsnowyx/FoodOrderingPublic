using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.Get;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, IEnumerable<OrderResponse>>
{
    private readonly IOrdersRepository _repo;

    public GetOrdersQueryHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<OrderResponse>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var all = await _repo.GetAllAsync(cancellationToken);
        return all.Select(OrderResponseFactory.Create);
    }
}
