using Application.Abstractions.Services;
using Application.DTOs.Payment;
using MediatR;

namespace Application.Features.Tracking.PaymentUrlStatus;

public class PaymentUrlStatusOrderTrackingCommandHandler : IRequestHandler<PaymentUrlStatusOrderTrackingCommand, PaymentUrlStatusResult>
{
    private readonly IDeliveryTrackingAccessService _deliveryTrackingAccessService;
    private readonly IStripePaymentService _stripePaymentService;

    public PaymentUrlStatusOrderTrackingCommandHandler(
        IDeliveryTrackingAccessService deliveryTrackingAccessService,
        IStripePaymentService stripePaymentService)
    {
        _deliveryTrackingAccessService = deliveryTrackingAccessService;
        _stripePaymentService = stripePaymentService;
    }

    public async Task<PaymentUrlStatusResult> Handle(
        PaymentUrlStatusOrderTrackingCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _deliveryTrackingAccessService.GetGuestTrackedOrderAsync(
            request.TrackingToken,
            cancellationToken);

        return await _stripePaymentService.PaymentUrlStatusAsync(
            order,
            cancellationToken);
    }
}
