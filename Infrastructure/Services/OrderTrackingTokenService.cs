using Application.Abstractions.Services;
using Common.Constants;
using Domain.ConfigModels;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class OrderTrackingTokenService : IOrderTrackingTokenService
{
    private static readonly TimeSpan DefaultLifetime = TimeSpan.FromDays(30);
    private const string TrackingTokenUrlPlaceholder = "{trackingToken}";

    private readonly OrderTrackingSettings _settings;

    public OrderTrackingTokenService(IOptions<OrderTrackingSettings> options)
    {
        _settings = options.Value;
    }

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

    public string CreatePublicTrackingUrl(string rawToken)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            throw new ArgumentException("Tracking token cannot be empty.", nameof(rawToken));
        }

        if (string.IsNullOrWhiteSpace(_settings.PublicUrl))
        {
            throw new InvalidOperationException(
                "OrderTrackingSettings:PublicUrl is required.");
        }

        if (!_settings.PublicUrl.Contains(TrackingTokenUrlPlaceholder, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"OrderTrackingSettings:PublicUrl must include the {TrackingTokenUrlPlaceholder} placeholder.");
        }

        var trackingUrl = _settings.PublicUrl.Replace(
            TrackingTokenUrlPlaceholder,
            Uri.EscapeDataString(rawToken),
            StringComparison.Ordinal);

        if (!Uri.TryCreate(trackingUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "OrderTrackingSettings:PublicUrl must be an absolute HTTP or HTTPS URL.");
        }

        return uri.ToString();
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
