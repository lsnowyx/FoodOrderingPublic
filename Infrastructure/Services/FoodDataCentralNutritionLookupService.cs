using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Abstractions.Services;
using Application.Caching;
using Application.DTOs.Ingredient;
using Domain.ConfigModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

public sealed class FoodDataCentralNutritionLookupService : INutritionLookupService
{
    private const string Source = "FoodDataCentral";
    private const string BaseUnit = "100g";
    private const int SafeDefaultPageSize = 10;
    private const int MaxPageSize = 50;
    private const int CaloriesNutrientId = 1008;
    private const string CaloriesNutrientNumber = "208";
    private const string FoundationDataType = "Foundation";
    private const string SrLegacyDataType = "SR Legacy";
    private const string SurveyDataType = "Survey (FNDDS)";
    private const string BrandedDataType = "Branded";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromDays(7);

    private readonly HttpClient _httpClient;
    private readonly ICacheService _cacheService;
    private readonly ILogger<FoodDataCentralNutritionLookupService> _logger;
    private readonly NutritionLookupSettings _settings;

    public FoodDataCentralNutritionLookupService(
        HttpClient httpClient,
        ICacheService cacheService,
        ILogger<FoodDataCentralNutritionLookupService> logger,
        IOptions<NutritionLookupSettings> options)
    {
        _httpClient = httpClient;
        _cacheService = cacheService;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task<IReadOnlyList<NutritionLookupResult>> SearchAsync(
        string query,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Nutrition search query is required.", nameof(query));
        }

        EnsureConfigured();

        var trimmedQuery = query.Trim();
        var normalizedPageSize = NormalizePageSize(pageSize);
        var cacheKey = CacheKeyHelper.NutritionSearch(trimmedQuery, normalizedPageSize);
        var cached = await _cacheService.GetAsync<List<NutritionLookupResult>>(
            cacheKey,
            cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var requestUri = CreateSearchUri(trimmedQuery, normalizedPageSize);

        try
        {
            using var response = await _httpClient.GetAsync(requestUri, cancellationToken);
            LogRateLimitHeaders(trimmedQuery, response);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    "Nutrition lookup service is unavailable. Please try again later.");
            }

            var searchResponse = await response.Content
                .ReadFromJsonAsync<FoodDataCentralSearchResponse>(cancellationToken);

            var results = MapResults(searchResponse, trimmedQuery).ToList();
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
                "Nutrition lookup service timed out. Please try again later.");
        }
        catch (HttpRequestException exception)
        {
            throw new InvalidOperationException(
                "Nutrition lookup service is unavailable. Please try again later.",
                exception);
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Nutrition lookup service returned an unexpected response.",
                exception);
        }
    }

    private void EnsureConfigured()
    {
        if (!string.Equals(_settings.Provider, Source, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Configured nutrition lookup provider is not supported.");
        }

        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            throw new InvalidOperationException("Nutrition lookup base URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Nutrition lookup API key is not configured.");
        }
    }

    private int NormalizePageSize(int pageSize)
    {
        var configuredDefault = _settings.DefaultPageSize > 0
            ? _settings.DefaultPageSize
            : SafeDefaultPageSize;

        var requestedPageSize = pageSize > 0 ? pageSize : configuredDefault;
        return Math.Min(requestedPageSize, MaxPageSize);
    }

    private string CreateSearchUri(string query, int pageSize)
    {
        var baseUrl = _settings.BaseUrl.TrimEnd('/');
        var queryString = string.Join(
            "&",
            $"query={Uri.EscapeDataString(query)}",
            $"pageSize={pageSize}",
            $"api_key={Uri.EscapeDataString(_settings.ApiKey)}");

        return $"{baseUrl}/foods/search?{queryString}";
    }

    private static IReadOnlyList<NutritionLookupResult> MapResults(
        FoodDataCentralSearchResponse? response,
        string query)
    {
        if (response?.Foods is null || response.Foods.Count == 0)
        {
            return Array.Empty<NutritionLookupResult>();
        }

        var normalizedQuery = NormalizeText(query);
        var queryWords = SplitWords(normalizedQuery);

        return response.Foods
            .Where(food => food.FdcId.HasValue && !string.IsNullOrWhiteSpace(food.Description))
            .Select(food => CreateRankedResult(food, normalizedQuery, queryWords))
            .OrderBy(ranked => ranked.DataTypeRank)
            .ThenBy(ranked => ranked.CaloriesRank)
            .ThenBy(ranked => ranked.MatchRank)
            .ThenBy(ranked => ranked.PreparedNameRank)
            .ThenBy(ranked => ranked.WordCount)
            .ThenBy(ranked => ranked.Name, StringComparer.OrdinalIgnoreCase)
            .Select(ranked => ranked.Result)
            .ToList();
    }

    private static RankedNutritionLookupResult CreateRankedResult(
        FoodDataCentralFood food,
        string normalizedQuery,
        IReadOnlyCollection<string> queryWords)
    {
        var result = new NutritionLookupResult(
            food.FdcId!.Value.ToString(),
            food.Description!.Trim(),
            TrimToNull(food.BrandName),
            TrimToNull(food.DataType),
            BaseUnit,
            ExtractCalories(food.FoodNutrients),
            Source);

        var normalizedName = NormalizeText(result.Name);
        var nameWords = SplitWords(normalizedName);

        return new RankedNutritionLookupResult(
            result,
            GetDataTypeRank(result.DataType),
            result.CaloriesPerUnit.HasValue ? 0 : 1,
            GetMatchRank(normalizedName, nameWords, normalizedQuery, queryWords),
            IsPreparedMealLikeName(normalizedName) ? 1 : 0,
            nameWords.Count,
            result.Name);
    }

    private static int GetDataTypeRank(string? dataType)
    {
        return dataType switch
        {
            FoundationDataType => 0,
            SrLegacyDataType => 1,
            null => 3,
            SurveyDataType => 4,
            BrandedDataType => 5,
            _ => 2
        };
    }

    private static int GetMatchRank(
        string normalizedName,
        IReadOnlyCollection<string> nameWords,
        string normalizedQuery,
        IReadOnlyCollection<string> queryWords)
    {
        if (string.Equals(normalizedName, normalizedQuery, StringComparison.Ordinal))
        {
            return 0;
        }

        if (nameWords.Any(word => IsSameIngredientWord(word, normalizedQuery)))
        {
            return 1;
        }

        if (normalizedName.StartsWith(normalizedQuery, StringComparison.Ordinal))
        {
            return 2;
        }

        if (queryWords.Count > 0
            && queryWords.All(queryWord => nameWords.Any(nameWord => IsSameIngredientWord(nameWord, queryWord))))
        {
            return 3;
        }

        if (normalizedName.Contains(normalizedQuery, StringComparison.Ordinal))
        {
            return 4;
        }

        return 5;
    }

    private static bool IsPreparedMealLikeName(string normalizedName)
    {
        var preparedIndicators = new[]
        {
            "with",
            "roll",
            "salad",
            "sandwich",
            "pizza",
            "soup",
            "stew",
            "casserole",
            "benedict",
            "burrito",
            "taco",
            "wrap",
            "fried rice",
            "prepared",
            "meal",
            "dinner",
            "fast food"
        };

        return preparedIndicators.Any(indicator =>
            normalizedName.Contains(indicator, StringComparison.Ordinal));
    }

    private static bool IsSameIngredientWord(string word, string query)
    {
        return string.Equals(word, query, StringComparison.Ordinal)
            || string.Equals(RemoveTrailingPlural(word), query, StringComparison.Ordinal)
            || string.Equals(word, RemoveTrailingPlural(query), StringComparison.Ordinal);
    }

    private static string RemoveTrailingPlural(string value)
    {
        return value.Length > 1 && value.EndsWith('s')
            ? value[..^1]
            : value;
    }

    private static List<string> SplitWords(string value)
    {
        return value
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static string NormalizeText(string value)
    {
        var chars = value
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : ' ')
            .ToArray();

        return string.Join(
            ' ',
            new string(chars).Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private void LogRateLimitHeaders(string query, HttpResponseMessage response)
    {
        var rateLimit = GetHeaderValue(response, "X-RateLimit-Limit")
            ?? GetHeaderValue(response, "RateLimit-Limit");
        var remaining = GetHeaderValue(response, "X-RateLimit-Remaining")
            ?? GetHeaderValue(response, "RateLimit-Remaining");
        var reset = GetHeaderValue(response, "X-RateLimit-Reset")
            ?? GetHeaderValue(response, "RateLimit-Reset");

        _logger.LogInformation(
            "FoodDataCentral request completed. Query={Query}, StatusCode={StatusCode}, RateLimit={RateLimit}, Remaining={Remaining}, Reset={Reset}",
            query,
            (int)response.StatusCode,
            rateLimit,
            remaining,
            reset);
    }

    private static string? GetHeaderValue(HttpResponseMessage response, string headerName)
    {
        if (response.Headers.TryGetValues(headerName, out var values))
        {
            return values.FirstOrDefault();
        }

        return response.Content.Headers.TryGetValues(headerName, out var contentValues)
            ? contentValues.FirstOrDefault()
            : null;
    }

    private static int? ExtractCalories(IReadOnlyCollection<FoodDataCentralNutrient>? nutrients)
    {
        if (nutrients is null || nutrients.Count == 0)
        {
            return null;
        }

        var nutrient = nutrients.FirstOrDefault(IsCaloriesNutrient);
        if (nutrient?.Value is null)
        {
            return null;
        }

        return (int)Math.Round(nutrient.Value.Value, MidpointRounding.AwayFromZero);
    }

    private static bool IsCaloriesNutrient(FoodDataCentralNutrient nutrient)
    {
        if (nutrient.Value is null)
        {
            return false;
        }

        var isEnergy =
            nutrient.NutrientId == CaloriesNutrientId
            || string.Equals(nutrient.NutrientNumber, CaloriesNutrientNumber, StringComparison.OrdinalIgnoreCase)
            || string.Equals(nutrient.NutrientName, "Energy", StringComparison.OrdinalIgnoreCase);

        var isKcal = string.Equals(nutrient.UnitName, "KCAL", StringComparison.OrdinalIgnoreCase);

        return isEnergy && isKcal;
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed class FoodDataCentralSearchResponse
    {
        [JsonPropertyName("foods")]
        public List<FoodDataCentralFood> Foods { get; set; } = new();
    }

    private sealed class FoodDataCentralFood
    {
        [JsonPropertyName("fdcId")]
        public long? FdcId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("brandName")]
        public string? BrandName { get; set; }

        [JsonPropertyName("dataType")]
        public string? DataType { get; set; }

        [JsonPropertyName("foodNutrients")]
        public List<FoodDataCentralNutrient>? FoodNutrients { get; set; }
    }

    private sealed class FoodDataCentralNutrient
    {
        [JsonPropertyName("nutrientId")]
        public int? NutrientId { get; set; }

        [JsonPropertyName("nutrientName")]
        public string? NutrientName { get; set; }

        [JsonPropertyName("nutrientNumber")]
        public string? NutrientNumber { get; set; }

        [JsonPropertyName("unitName")]
        public string? UnitName { get; set; }

        [JsonPropertyName("value")]
        public decimal? Value { get; set; }
    }

    private sealed record RankedNutritionLookupResult(
        NutritionLookupResult Result,
        int DataTypeRank,
        int CaloriesRank,
        int MatchRank,
        int PreparedNameRank,
        int WordCount,
        string Name);
}
