using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    private readonly IOrdersRepository _repo;

    public GetOrderByIdQueryHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (o == null) return null;
        return OrderResponseFactory.Create(o);
    }
}
