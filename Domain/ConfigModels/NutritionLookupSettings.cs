namespace Domain.ConfigModels;

public class NutritionLookupSettings
{
    public string Provider { get; set; } = "FoodDataCentral";

    public string BaseUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public int DefaultPageSize { get; set; } = 10;
}
