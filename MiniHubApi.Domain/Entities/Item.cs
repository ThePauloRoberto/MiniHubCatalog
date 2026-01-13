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
    public double Preco { get; set; }
    public bool Ativo { get; set; }
    public Guid? CategoryId { get; set; }
    public Category Categoria { get; set; }
    public List<Tag> Tags { get; set; }
    
}
