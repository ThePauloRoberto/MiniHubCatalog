using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MiniHubApi.Domain.Entities;

public class Item
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; } 
    public decimal Preco { get; set; }
    public bool Ativo { get; set; } = true;
    public int? CategoryId { get; set; }
    public Category? Categoria { get; set; }
    public List<Tag> Tags { get; set; }
    public int Estoque { get; set; } = 0;
    
    public string? ExternalId { get; set; } 
    public string? CategoryExternalId { get; set; }

    
}
