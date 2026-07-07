namespace Domain.ConfigModels;

public class NominatimSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public int DefaultLimit { get; set; } = 5;

    public string CountryCodes { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public string ViewBox { get; set; } = string.Empty;

    public bool Bounded { get; set; }

    public string RequiredDisplayNameTerm { get; set; } = string.Empty;
}
