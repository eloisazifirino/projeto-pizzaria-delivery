using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("atendimentoitem")]
public class AtendimentoItem
{
    [Key]
    [Column("id_atendimento_item")]
    public int IdAtendimentoItem { get; set; }

    [Column("id_atendimento")]
    public int IdAtendimento { get; set; }

    [Column("id_item")]
    public int IdItem { get; set; }

    [Column("quantidade")]
    public int Quantidade { get; set; }

    [Column("preco_unitario")]
    public decimal PrecoUnitario { get; set; }
}