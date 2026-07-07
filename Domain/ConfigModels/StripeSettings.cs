namespace Domain.ConfigModels;

public class StripeSettings
{
    public const string DefaultCurrency = "ron";

    private readonly string currency = DefaultCurrency;

    public string SecretKey { get; init; } = null!;
    public string WebhookSecret { get; init; } = null!;
    public string SuccessUrl { get; init; } = null!;
    public string CancelUrl { get; init; } = null!;
    public string Currency
    {
        get => string.IsNullOrWhiteSpace(currency)
            ? DefaultCurrency
            : currency.Trim().ToLowerInvariant();
        init => currency = value;
    }
}
