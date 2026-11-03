using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("item")]
public class Item
{
    [Key]
    [Column("iditem")]
    public int IdItem { get; set; }

    [Column("idcategoria")]
    public int? IdCategoria { get; set; }

    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Column("descricao_item")]
    public string? DescricaoItem { get; set; }

    [Column("preco")]
    public decimal Preco { get; set; }

    [Column("tipo_item")]
    public string TipoItem { get; set; } = "P";

    [Column("ativo_produto_servico")]
    public string AtivoProdutoServico { get; set; } = "S";
}