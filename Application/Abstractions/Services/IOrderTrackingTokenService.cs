using Common.Constants;
using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IOrderTrackingTokenService
{
    string GenerateRawToken();

    string HashToken(string rawToken);

    string CreatePublicTrackingUrl(string rawToken);

    OrderTrackingLink CreateTrackingLink(
        Order order,
        string rawToken,
        TimeSpan? lifetime = null,
        string scope = TrackingScopeConstants.OrderTrackingRead);
}
