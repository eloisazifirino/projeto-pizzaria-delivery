using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("atendimento")]
public class Atendimento
{
    [Key]
    [Column("idatendimento")]
    public int IdAtendimento { get; set; }

    [Column("data_atendimento")]
    public DateTime DataHora { get; set; } = DateTime.Now;

    [NotMapped]
    public decimal ValorTotal => AtendimentoItens.Sum(item => item.PrecoUnitario * item.Quantidade);

    [Column("status_atendimento")]
    public string Status { get; set; } = "Em Processamento";

    [Column("observacao")]
    public string? Observacao { get; set; }

    public List<AtendimentoItem> AtendimentoItens { get; set; } = new();
}