using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace MiniHubApi.Domain.Entities
{
    [Table("Items")]
    public class Item
    {
        Guid Id { get; set; }
        string Nome { get; set; }
        string Descricao { get; set; }
        string Categoria { get; set; }
        double Preco { get; set; }
        bool Ativo { get; set; }
        string Tags { get; set; }
    }
}
