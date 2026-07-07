using Common.Enums;

namespace Application.Features.Order;

internal static class OrderPaymentDisplay
{
    public static PaymentStatus GetEffectivePaymentStatus(Domain.Entities.Order order)
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

    public static bool CanPayOnlineAgain(Domain.Entities.Order order)
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

    public static string GetPaymentMessage(Domain.Entities.Order order)
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
