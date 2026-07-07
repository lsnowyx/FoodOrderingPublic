using Application.Abstractions.Repositories;
using Application.DTOs.Order;
using Mapster;
using MediatR;

namespace Application.Features.Order.GetCustomerOrderDetails;

public sealed class GetCustomerOrderDetailsQueryHandler
    : IRequestHandler<GetCustomerOrderDetailsQuery, CustomerOrderDetailsResponse?>
{
    private readonly IOrdersRepository _ordersRepository;

    public GetCustomerOrderDetailsQueryHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<CustomerOrderDetailsResponse?> Handle(
        GetCustomerOrderDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var order = await _ordersRepository.GetCustomerOrderDetailsAsync(
            request.OrderId,
            request.CustomerId,
            cancellationToken);

        return order?.Adapt<CustomerOrderDetailsResponse>();
    }
}
