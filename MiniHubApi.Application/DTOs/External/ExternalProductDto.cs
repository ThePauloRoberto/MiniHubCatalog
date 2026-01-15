using System.Text.Json.Serialization;

namespace MiniHubApi.Application.DTOs.External;

public class ExternalProductDto
{
    // [JsonPropertyName] diz qual é o nome no JSON
    [JsonPropertyName("id")]  // No JSON vem como "id"
    public string ExternalId { get; set; } = string.Empty;  // Aqui chamamos de ExternalId
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public string PriceString  { get; set; } = string.Empty;  // Vem como string "178.09"
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("Stock")]  // Note: "S" maiúsculo na API!
    public int Stock { get; set; }
    
    [JsonPropertyName("CategoryId")]
    public string CategoryExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("tag1")]
    public string? Tag1 { get; set; }
    
    [JsonPropertyName("tag2")]
    public string? Tag2 { get; set; }
    
    [JsonPropertyName("tag3")]
    public string? Tag3 { get; set; }
    
    [JsonIgnore]
    public decimal Price 
    { 
        get 
        {
            if (decimal.TryParse(PriceString, out var price))
                return price;
            return 0;
        }
    }

    public List<string> GetTags()
    {
        var tags = new List<string>();

        if (!string.IsNullOrWhiteSpace(Tag1))
        {
            tags.Add(Tag1.Trim());
        }
        if (!string.IsNullOrWhiteSpace(Tag2))
        {
            tags.Add(Tag2.Trim());
        }
        if (!string.IsNullOrWhiteSpace(Tag3))
        {
            tags.Add(Tag3.Trim());
        }

        return tags.Distinct().ToList();
    }
}
