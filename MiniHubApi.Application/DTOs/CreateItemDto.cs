namespace MiniHubApi.Application.DTOs;

public class CreateItemDto
{
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
    public int Estoque { get; set; } = 0;
    public int? CategoryId { get; set; }
    public List<Guid> TagIds { get; set; } = new(); 
}