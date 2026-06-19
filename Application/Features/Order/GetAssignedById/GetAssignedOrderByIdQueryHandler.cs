using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.GetAssignedById;

public sealed class GetAssignedOrderByIdQueryHandler
    : IRequestHandler<GetAssignedOrderByIdQuery, OrderResponse?>
{
    private readonly IOrdersRepository _repository;

    public GetAssignedOrderByIdQueryHandler(IOrdersRepository repository)
    {
        _repository = repository;
    }

    public async Task<OrderResponse?> Handle(
        GetAssignedOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null)
        {
            return null;
        }

        OrderAssignmentGuard.EnsureAssignedToOrderManager(order, request.OrderManagerId);

        return OrderResponseFactory.Create(order);
    }
}
