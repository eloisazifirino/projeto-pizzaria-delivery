using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("atendimentoitem")]
public class AtendimentoItem
{
    [Key]
    [Column("idatendimentoitem")]
    public int IdAtendimentoItem { get; set; }

    [Column("idatendimento")]
    public int IdAtendimento { get; set; }

    [Column("iditem")]
    public int IdItem { get; set; }

    [Column("quantidade_itens")]
    public int Quantidade { get; set; }

    [Column("valor_unitario")]
    public decimal PrecoUnitario { get; set; }
}