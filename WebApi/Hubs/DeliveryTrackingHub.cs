using Application.Abstractions.Services;
using Common.Constants;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Hubs;

public sealed class DeliveryTrackingHub : Hub
{
    private readonly IDeliveryTrackingAccessService _deliveryTrackingAccessService;

    public DeliveryTrackingHub(
        IDeliveryTrackingAccessService deliveryTrackingAccessService)
    {
        _deliveryTrackingAccessService = deliveryTrackingAccessService;
    }

    public async Task SubscribeAsGuest(Guid orderId, string trackingToken)
    {
        await _deliveryTrackingAccessService.GetGuestTrackedOrderAsync(
            orderId,
            trackingToken,
            Context.ConnectionAborted);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            DeliveryTrackingGroups.ForOrder(orderId),
            Context.ConnectionAborted);
    }

    [Authorize(AuthorizationPolicyConstants.USER_POLICY)]
    public async Task SubscribeAsCustomer(Guid orderId)
    {
        var customerId = Context.User!.GetUserId();
        var order = await _deliveryTrackingAccessService.GetCustomerTrackedOrderAsync(
            orderId,
            customerId,
            Context.ConnectionAborted);

        if (order is null)
        {
            throw new HubException("Order was not found.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            DeliveryTrackingGroups.ForOrder(orderId),
            Context.ConnectionAborted);
    }
}
