using Application.Abstractions.Services;
using Application.DTOs.Order;
using MediatR;

namespace Application.Features.Order.StartDelivery;

public class StartDeliveryCommandHandler : IRequestHandler<StartDeliveryCommand, OrderResponse>
{
    private readonly IStartDeliveryService _startDeliveryService;

    public StartDeliveryCommandHandler(IStartDeliveryService startDeliveryService)
    {
        _startDeliveryService = startDeliveryService;
    }

    public async Task<OrderResponse> Handle(StartDeliveryCommand request, CancellationToken cancellationToken)
    {
        var order = await _startDeliveryService.StartAsync(
            request.Id,
            request.OrderManagerId,
            cancellationToken);

        return OrderResponseFactory.Create(order);
    }
}
