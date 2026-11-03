using System.ComponentModel.DataAnnotations;

namespace PizzariaApi.DTOs;

public record CriarPedidoDto(
    string? Observacao,
    [Required, MinLength(1)] List<ItemPedidoDto> AtendimentoItens);

public record ItemPedidoDto(
    [Range(1, int.MaxValue)] int IdItem,
    [Range(1, int.MaxValue)] int Quantidade);
