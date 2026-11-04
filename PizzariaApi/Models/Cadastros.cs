using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PizzariaApi.Models;

[Table("categoria")]
public class Categoria
{
    [Column("grupo_cardapio")]
    public string GrupoCardapio { get; set; } = "outros";
    [Key, Column("idcategoria")]
    public int Id { get; set; }
    [Column("categoria")]
    public string Nome { get; set; } = "";
}

[Table("pessoa")]
public class Pessoa
{
    [Key, Column("idpessoa")]
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefone { get; set; }
    [Column("tipo_pessoa")]
    public string Tipo { get; set; } = "C";
    [Column("token_pedidos")]
    public string? TokenPedidos { get; set; }
    [Column("senha_hash")]
    public string? SenhaHash { get; set; }
}

[Table("entrada_produto")]
public class EntradaProduto
{
    public int Id { get; set; }
    [Column("iditem")]
    public int IdItem { get; set; }
    public int Quantidade { get; set; }
    [Column("data_entrada")]
    public DateOnly Data { get; set; }
    [Column("valor_unitario")]
    public decimal ValorUnitario { get; set; }
}
