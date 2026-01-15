namespace MiniHubApi.Application.DTOs.External;

public class ItemDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Ativo { get; set; }
    public int Estoque { get; set; }
    public string? ExternalId { get; set; }
    public string? CategoryExternalId { get; set; }
    public int? CategoryId { get; set; }
    public CategoryDto? Categoria { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}