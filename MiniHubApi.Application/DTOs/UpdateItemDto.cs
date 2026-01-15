namespace MiniHubApi.Application.DTOs;

public class UpdateItemDto
{
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public decimal? Preco { get; set; } 
    public bool? Ativo { get; set; } 
    public int? Estoque { get; set; }
    public int? CategoryId { get; set; } 
    public List<Guid>? TagIds { get; set; }  
}