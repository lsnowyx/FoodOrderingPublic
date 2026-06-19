using Application.Abstractions.Services;
using Common.Constants;
using Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class OrderTrackingTokenService : IOrderTrackingTokenService
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(30);

    public string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public string HashToken(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw new ArgumentException("Tracking token cannot be empty.", nameof(rawToken));
        }

        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public OrderTrackingLink CreateTrackingLink(
        Order order,
        string rawToken,
        TimeSpan? lifetime = null,
        string scope = TrackingScopeConstants.OrderTrackingRead)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (string.IsNullOrWhiteSpace(scope))
        {
            throw new ArgumentException("Tracking scope cannot be empty.", nameof(scope));
        }

        var effectiveLifetime = lifetime ?? DefaultLifetime;
        if (effectiveLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "Tracking link lifetime must be positive.");
        }

        var now = DateTime.UtcNow;

        return new OrderTrackingLink
        {
            OrderId = order.Id,
            Order = order,
            TokenHash = HashToken(rawToken),
            Scope = scope,
            CreatedAt = now,
            ExpiresAt = now.Add(effectiveLifetime)
        };
    }
}
