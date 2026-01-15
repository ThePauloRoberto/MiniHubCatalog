namespace MiniHubApi.Application.Configuration;

public class ExternalApiSettings
{
    public const string SectionName = "ExternalApi";  // Nome no appsettings.json
        
    public string BaseUrl { get; set; } = string.Empty;
    public string ProductsEndpoint { get; set; } = string.Empty;
    public string CategoriesEndpoint { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}