namespace Common.Constants;

public static class DeliveryTrackingGroups
{
    public static string ForOrder(Guid orderId)
    {
        return $"delivery-tracking-{orderId}";
    }
}
