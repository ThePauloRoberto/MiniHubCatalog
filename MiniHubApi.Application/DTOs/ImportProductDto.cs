namespace MiniHubApi.Application.DTOs;

public class ImportProductDto
{
    public string ExternalId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
    public int Estoque { get; set; } = 0;
    public string? CategoryExternalId { get; set; }
}