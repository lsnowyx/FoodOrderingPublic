using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetAvailable;

public class GetAvailableOrdersQueryHandler : IRequestHandler<GetAvailableOrdersQuery, IEnumerable<OrderResponse>>
{
    private readonly IOrdersRepository _repo;

    public GetAvailableOrdersQueryHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<OrderResponse>> Handle(GetAvailableOrdersQuery request, CancellationToken cancellationToken)
    {
        var all = await _repo.GetAvailableAsync(cancellationToken);
        return all.Select(OrderResponseFactory.Create);
    }
}
