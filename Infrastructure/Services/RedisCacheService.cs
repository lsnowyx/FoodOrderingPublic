using Application.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services;

public sealed class RedisCacheService : ICacheService
{
    private const string RegistryKey = "cache:key-registry";
    private static readonly TimeSpan CircuitBreakDuration = TimeSpan.FromMinutes(1);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _cache;
    private readonly CacheCircuitState _circuitState;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        CacheCircuitState circuitState,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _circuitState = circuitState;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken)
    {
        if (_circuitState.IsUnavailable(DateTimeOffset.UtcNow))
        {
            return default;
        }

        string? cached;
        try
        {
            cached = await _cache.GetStringAsync(key, cancellationToken);
            _circuitState.MarkAvailable();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            MarkCacheUnavailable(
                exception,
                "Cache read failed. Key={CacheKey}",
                key);
            return default;
        }

        if (string.IsNullOrWhiteSpace(cached))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cached, JsonOptions);
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Cached value could not be deserialized. Key={CacheKey}",
                key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        if (_circuitState.IsUnavailable(DateTimeOffset.UtcNow))
        {
            return;
        }

        string serialized;
        try
        {
            serialized = JsonSerializer.Serialize(value, JsonOptions);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Cache value could not be serialized. Key={CacheKey}",
                key);
            return;
        }

        try
        {
            await _cache.SetStringAsync(
                key,
                serialized,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                cancellationToken);

            await TrackKeyAsync(key, cancellationToken);
            _circuitState.MarkAvailable();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            MarkCacheUnavailable(
                exception,
                "Cache write failed. Key={CacheKey}",
                key);
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken)
    {
        if (_circuitState.IsUnavailable(DateTimeOffset.UtcNow))
        {
            return;
        }

        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            await UntrackKeyAsync(key, cancellationToken);
            _circuitState.MarkAvailable();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            MarkCacheUnavailable(
                exception,
                "Cache remove failed. Key={CacheKey}",
                key);
        }
    }

    public async Task RemoveByPrefixAsync(
        string prefix,
        CancellationToken cancellationToken)
    {
        if (_circuitState.IsUnavailable(DateTimeOffset.UtcNow))
        {
            return;
        }

        try
        {
            var keys = await GetTrackedKeysAsync(cancellationToken);
            var matchingKeys = keys
                .Where(key => key.StartsWith(prefix, StringComparison.Ordinal))
                .ToList();

            foreach (var key in matchingKeys)
            {
                await _cache.RemoveAsync(key, cancellationToken);
                keys.Remove(key);
            }

            await SaveTrackedKeysAsync(keys, cancellationToken);
            _circuitState.MarkAvailable();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            MarkCacheUnavailable(
                exception,
                "Cache prefix remove failed. Prefix={CachePrefix}",
                prefix);
        }
    }

    private async Task TrackKeyAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var keys = await GetTrackedKeysAsync(cancellationToken);
        if (keys.Add(key))
        {
            await SaveTrackedKeysAsync(keys, cancellationToken);
        }
    }

    private async Task UntrackKeyAsync(
        string key,
        CancellationToken cancellationToken)
    {
        var keys = await GetTrackedKeysAsync(cancellationToken);
        if (keys.Remove(key))
        {
            await SaveTrackedKeysAsync(keys, cancellationToken);
        }
    }

    private async Task<HashSet<string>> GetTrackedKeysAsync(
        CancellationToken cancellationToken)
    {
        var cached = await _cache.GetStringAsync(RegistryKey, cancellationToken);
        if (string.IsNullOrWhiteSpace(cached))
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        return JsonSerializer.Deserialize<HashSet<string>>(cached, JsonOptions)
            ?? new HashSet<string>(StringComparer.Ordinal);
    }

    private Task SaveTrackedKeysAsync(
        HashSet<string> keys,
        CancellationToken cancellationToken)
    {
        var serialized = JsonSerializer.Serialize(keys, JsonOptions);
        return _cache.SetStringAsync(RegistryKey, serialized, cancellationToken);
    }

    private void MarkCacheUnavailable(
        Exception exception,
        string message,
        string value)
    {
        var unavailableUntil = _circuitState.MarkUnavailable(
            DateTimeOffset.UtcNow,
            CircuitBreakDuration);

        _logger.LogWarning(
            exception,
            $"{message}. Redis cache will be bypassed until {{UnavailableUntilUtc}}.",
            value,
            unavailableUntil);
    }
}
