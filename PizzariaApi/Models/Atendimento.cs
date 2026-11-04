using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("atendimento")]
public class Atendimento
{
    [Key]
    [Column("idatendimento")]
    public int IdAtendimento { get; set; }

    [Column("idpessoa")]
    public int? IdPessoa { get; set; }

    [Column("data_atendimento")]
    public DateTime DataHora { get; set; } = DateTime.Now;

    [NotMapped]
    public decimal ValorTotal => TaxaEntrega + AtendimentoItens.Sum(item => item.PrecoUnitario * item.Quantidade);

    public string? Modalidade { get; set; }

    [Column("taxa_entrega")]
    public decimal TaxaEntrega { get; set; }

    [Column("status_atendimento")]
    public string Status { get; set; } = "Em Processamento";

    [Column("observacao")]
    public string? Observacao { get; set; }

    [Column("estoque_baixado")]
    [System.Text.Json.Serialization.JsonIgnore]
    public bool EstoqueBaixado { get; set; }

    public List<AtendimentoItem> AtendimentoItens { get; set; } = new();
}