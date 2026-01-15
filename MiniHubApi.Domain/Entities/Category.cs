namespace MiniHubApi.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? ExternalId { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
    
    public Category()
    {}
}