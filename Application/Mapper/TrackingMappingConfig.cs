using Application.DTOs.Tracking;
using Application.Features.Tracking.Get;
using Common.Enums;
using Domain.Entities;
using Mapster;

namespace Application.Mapper;

public class TrackingMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<GetOrderTrackingRequest, GetOrderTrackingQuery>();

        config.NewConfig<Order, OrderTrackingResponse>()
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.DeliveryStatus, src => src.Status.ToString())
            .Map(dest => dest.PaymentMethod, src => src.PaymentMethod.ToString())
            .Map(dest => dest.PaymentStatus, src => GetEffectivePaymentStatus(src).ToString())
            .Map(dest => dest.IsPaid, src => src.IsPaid)
            .Map(dest => dest.CanPayOnlineAgain, src => CanPayOnlineAgain(src))
            .Map(dest => dest.PaymentMessage, src => GetPaymentMessage(src))
            .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
            .Map(dest => dest.DeliveredAt, src => src.DeliveredAt)
            .Map(dest => dest.Items, src => src.OrderItems);
    }

    private static PaymentStatus GetEffectivePaymentStatus(Order order)
    {
        if (order.IsPaid)
        {
            return PaymentStatus.Paid;
        }

        if (order.PaymentMethod != PaymentMethod.OnlineCard)
        {
            return order.PaymentStatus;
        }

        var latestAttemptStatus = order.PaymentAttempts
            .OrderByDescending(paymentAttempt => paymentAttempt.CreatedAt)
            .Select(paymentAttempt => (PaymentStatus?)paymentAttempt.Status)
            .FirstOrDefault();

        return latestAttemptStatus is PaymentStatus.Failed or PaymentStatus.Expired
            ? latestAttemptStatus.Value
            : order.PaymentStatus;
    }

    private static bool CanPayOnlineAgain(Order order)
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

        return GetEffectivePaymentStatus(order) is PaymentStatus.PendingOnlinePayment
            or PaymentStatus.Failed
            or PaymentStatus.Expired
            or PaymentStatus.Unpaid;
    }

    private static string GetPaymentMessage(Order order)
    {
        if (order.PaymentMethod == PaymentMethod.CashOnDelivery)
        {
            return order.IsPaid
                ? "Payment completed."
                : "Payment will be collected by courier.";
        }

        return GetEffectivePaymentStatus(order) switch
        {
            PaymentStatus.Paid => "Payment completed.",
            PaymentStatus.Failed or PaymentStatus.Expired => "Payment failed or expired. You can try again.",
            PaymentStatus.PendingOnlinePayment => "Online payment is pending.",
            _ => "Online payment is not completed."
        };
    }
}
