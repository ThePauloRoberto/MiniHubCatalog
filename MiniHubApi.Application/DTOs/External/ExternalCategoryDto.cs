using System.Text.Json.Serialization;

namespace MiniHubApi.Application.DTOs.External;

public class ExternalCategoryDto
{
    [JsonPropertyName("id")]
    public string ExternalId { get; set; } = string.Empty;
        
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
        
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
        
    [JsonPropertyName("tag1")]
    public string? Tag1 { get; set; }
        
    [JsonPropertyName("tag2")]
    public string? Tag2 { get; set; }
        
    [JsonPropertyName("tag3")]
    public string? Tag3 { get; set; }
}
