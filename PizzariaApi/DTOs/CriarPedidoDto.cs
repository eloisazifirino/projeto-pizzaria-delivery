using System.ComponentModel.DataAnnotations;

namespace PizzariaApi.DTOs;

public record CriarPedidoDto(
    [StringLength(1000)] string? Observacao,
    [Required, MinLength(1), MaxLength(100)] List<ItemPedidoDto> AtendimentoItens,
    [Required, RegularExpression("^(entrega|retirada)$")] string Modalidade);

public record ItemPedidoDto(
    [Range(1, int.MaxValue)] int IdItem,
    [Range(1, int.MaxValue)] int Quantidade,
    int? Fatias = null);
