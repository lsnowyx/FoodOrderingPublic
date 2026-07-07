using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Order;
using Common.Enums;
using MediatR;

namespace Application.Features.Order.GetCustomerPaymentLink;

public sealed class GetCustomerPaymentLinkCommandHandler
    : IRequestHandler<GetCustomerPaymentLinkCommand, CustomerPaymentUrlStatusResponse>
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IStripePaymentService _stripePaymentService;

    public GetCustomerPaymentLinkCommandHandler(
        IOrdersRepository ordersRepository,
        IStripePaymentService stripePaymentService)
    {
        _ordersRepository = ordersRepository;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<CustomerPaymentUrlStatusResponse> Handle(
        GetCustomerPaymentLinkCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _ordersRepository.GetOrderForCustomerPaymentRetryAsync(
            request.OrderId,
            request.CustomerId,
            cancellationToken);

        if (order is null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        if (!CanCallPaymentService(order))
        {
            return CreateResponse(
                order,
                null,
                OrderPaymentDisplay.GetPaymentMessage(order));
        }

        var paymentResult = await _stripePaymentService.PaymentUrlStatusAsync(
            order,
            cancellationToken);

        return CreateResponse(
            order,
            paymentResult.PaymentUrl,
            paymentResult.Message);
    }

    private static bool CanCallPaymentService(Domain.Entities.Order order)
    {
        if (order.IsPaid)
        {
            return false;
        }

        if (order.PaymentMethod != PaymentMethod.OnlineCard)
        {
            return false;
        }

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return false;
        }

        return OrderPaymentDisplay.GetEffectivePaymentStatus(order) is PaymentStatus.PendingOnlinePayment
            or PaymentStatus.Failed
            or PaymentStatus.Expired
            or PaymentStatus.Unpaid;
    }

    private static CustomerPaymentUrlStatusResponse CreateResponse(
        Domain.Entities.Order order,
        string? paymentUrl,
        string message)
    {
        return new CustomerPaymentUrlStatusResponse(
            order.IsPaid,
            OrderPaymentDisplay.CanPayOnlineAgain(order),
            paymentUrl,
            message,
            OrderPaymentDisplay.GetEffectivePaymentStatus(order).ToString());
    }
}
