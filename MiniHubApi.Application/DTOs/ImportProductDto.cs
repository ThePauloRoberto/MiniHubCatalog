using System.ComponentModel.DataAnnotations;

namespace MiniHubApi.Application.DTOs;

public class ImportProductDto
{
    [Required(ErrorMessage = "ExternalId é obrigatório")]
    public string ExternalId { get; set; } = string.Empty;
        
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(200, ErrorMessage = "Nome não pode exceder 200 caracteres")]
    public string Nome { get; set; } = string.Empty;
        
    [StringLength(500, ErrorMessage = "Descrição não pode exceder 500 caracteres")]
    public string Descricao { get; set; } = string.Empty;
        
    [Range(0, double.MaxValue, ErrorMessage = "Preço deve ser maior ou igual a 0")]
    public decimal Preco { get; set; }
        
    public bool Ativo { get; set; } = true;
        
    [Range(0, int.MaxValue, ErrorMessage = "Estoque deve ser maior ou igual a 0")]
    public int Estoque { get; set; } = 0;
        
    public string? CategoryExternalId { get; set; }
    
    public List<string> Tags { get; set; } = new List<string>();
    
}