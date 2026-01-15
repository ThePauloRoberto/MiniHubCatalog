namespace MiniHubApi.Application.DTOs;

public class CreateCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
}