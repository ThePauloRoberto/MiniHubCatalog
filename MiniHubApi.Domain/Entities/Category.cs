namespace MiniHubApi.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    public Item Item { get; set; }
    
    public Category()
    {}
}