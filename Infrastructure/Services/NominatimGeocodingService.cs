using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.Location;
using Domain.ConfigModels;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class NominatimGeocodingService : IGeocodingService
{
    private const int MinimumQueryLength = 3;
    private const int SafeDefaultLimit = 5;
    private const int MinimumLimit = 1;
    private const int MaximumLimit = 10;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly NominatimSettings _settings;

    public NominatimGeocodingService(
        HttpClient httpClient,
        ICacheService cacheService,
        IOptions<NominatimSettings> options)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<LocationSearchResult>> SearchAsync(
        string query,
        int? limit,
        CancellationToken cancellationToken = default)
    {
        var trimmedQuery = query.Trim();
        if (trimmedQuery.Length < MinimumQueryLength)
        {
            throw new ArgumentException(
                $"Location search query must contain at least {MinimumQueryLength} characters.",
                nameof(query));
        }

        EnsureConfigured();

        var normalizedLimit = NormalizeLimit(limit);
        var cacheKey = CacheKeyHelper.NominatimSearch(
            trimmedQuery,
            normalizedLimit,
            _settings.CountryCodes);
        var cached = await _cacheService.GetAsync<List<LocationSearchResult>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            CreateSearchUri(trimmedQuery, normalizedLimit));
        request.Headers.TryAddWithoutValidation("User-Agent", _settings.UserAgent);

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    "Location search service is unavailable. Please try again later.");
            }

            var searchResponse = await response.Content
                .ReadFromJsonAsync<List<NominatimSearchResult>>(cancellationToken);

            var results = MapResults(searchResponse).ToList();
            await _cacheService.SetAsync(
                cacheKey,
                results,
                CacheDuration,
                cancellationToken);

            return results;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "Location search service timed out. Please try again later.");
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException(
                "Location search service is unavailable. Please try again later.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Location search service returned an unexpected response.",
                exception);
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            throw new InvalidOperationException("Nominatim base URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.UserAgent))
        {
            throw new InvalidOperationException("Nominatim user agent is not configured.");
        }
    }

    private int NormalizeLimit(int? limit)
    {
        var defaultLimit = _settings.DefaultLimit > 0
            ? _settings.DefaultLimit
            : SafeDefaultLimit;

        var requestedLimit = limit.GetValueOrDefault(defaultLimit);
        return Math.Clamp(requestedLimit, MinimumLimit, MaximumLimit);
    }

    private string CreateSearchUri(string query, int limit)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var parameters = new List<string>
        {
            $"q={Uri.EscapeDataString(query)}",
            "format=json",
            $"limit={limit}",
            "addressdetails=1"
        };

        if (!string.IsNullOrWhiteSpace(_settings.CountryCodes))
        {
            parameters.Add($"countrycodes={Uri.EscapeDataString(_settings.CountryCodes)}");
        }

        if (!string.IsNullOrWhiteSpace(_settings.ViewBox))
        {
            parameters.Add($"viewbox={Uri.EscapeDataString(_settings.ViewBox)}");

            if (_settings.Bounded)
            {
                parameters.Add("bounded=1");
            }
        }

        return $"{baseUrl}/search?{string.Join("&", parameters)}";
    }

    private IReadOnlyList<LocationSearchResult> MapResults(
        IReadOnlyCollection<NominatimSearchResult>? results)
    {
        if (results is null || results.Count == 0)
        {
            return Array.Empty<LocationSearchResult>();
        }

        return results
            .Select(MapResult)
            .Where(result => result is not null && IsAllowedDisplayName(result.DisplayName))
            .Select(result => result!)
            .ToList();
    }

    private static LocationSearchResult? MapResult(NominatimSearchResult result)
    {
        if (string.IsNullOrWhiteSpace(result.DisplayName)
            || !TryParseCoordinate(result.Latitude, out var latitude)
            || !TryParseCoordinate(result.Longitude, out var longitude))
        {
            return null;
        }

        return new LocationSearchResult(
            result.DisplayName.Trim(),
            latitude,
            longitude,
            TrimToNull(result.Type),
            result.Importance);
    }

    private static bool TryParseCoordinate(string? value, out double coordinate)
    {
        return double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out coordinate);
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private bool IsAllowedDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(_settings.RequiredDisplayNameTerm))
        {
            return true;
        }

        return NormalizeForSearch(displayName)
            .Contains(
                NormalizeForSearch(_settings.RequiredDisplayNameTerm),
                StringComparison.Ordinal);
    }

    private static string NormalizeForSearch(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private sealed class NominatimSearchResult
    {
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("lat")]
        public string? Latitude { get; set; }

        [JsonPropertyName("lon")]
        public string? Longitude { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("importance")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public double? Importance { get; set; }
    }
}
