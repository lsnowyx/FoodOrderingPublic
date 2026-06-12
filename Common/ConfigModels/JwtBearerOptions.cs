namespace Common.ConfigModels;

public class JwtBearerOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required string Secret { get; set; }
    public required int AccessExpirationInMinutes { get; set; }

    public required int RefreshExpirationInMinutes { get; set; }
    public required int RefreshTokenLength { get; set; }
}