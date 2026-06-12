using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetMyDelivery;

public class GetMyDeliveryQueryHandler : IRequestHandler<GetMyDeliveryQuery, OrderResponse?>
{
    private readonly IOrdersRepository _repo;

    public GetMyDeliveryQueryHandler(IOrdersRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderResponse?> Handle(GetMyDeliveryQuery request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetActiveAssignedToOrderManagerAsync(request.OrderManagerId, cancellationToken);
        return order == null ? null : OrderResponseFactory.Create(order);
    }
}
