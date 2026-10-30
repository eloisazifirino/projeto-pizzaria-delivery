using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("atendimento")]
public class Atendimento
{
    [Key]
    [Column("id_atendimento")]
    public int IdAtendimento { get; set; }

    [Column("data_hora")]
    public DateTime DataHora { get; set; } = DateTime.Now;

    [Column("valor_total")]
    public decimal ValorTotal { get; set; }

    [Column("status")]
    public string Status { get; set; } = "Em Processamento";

    [Column("observacao")]
    public string? Observacao { get; set; }

    public List<AtendimentoItem> AtendimentoItens { get; set; } = new();
}