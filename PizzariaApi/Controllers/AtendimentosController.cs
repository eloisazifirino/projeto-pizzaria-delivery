using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;
using PizzariaApi.DTOs;
using PizzariaApi.Models;

namespace PizzariaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AtendimentosController : ControllerBase
{
    private readonly AppDbContext _context;

    public AtendimentosController(AppDbContext context)
    {
        _context = context;
    }

    // READ: Listar todos os pedidos
    [HttpGet]
    public async Task<IActionResult> GetPedidos([FromQuery] string? status)
    {
        var pedidos = await _context.Atendimentos
            .Where(a => status == null || a.Status == status)
            .Include(a => a.AtendimentoItens)
            .OrderByDescending(a => a.IdAtendimento)
            .ToListAsync();
        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPedido(int id)
    {
        var pedido = await _context.Atendimentos
            .Include(a => a.AtendimentoItens)
            .FirstOrDefaultAsync(a => a.IdAtendimento == id);
        return pedido == null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> CriarPedido(CriarPedidoDto dados,
        [FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();
        var cliente = await _context.Pessoas.SingleOrDefaultAsync(p => p.TokenPedidos == token && p.Tipo == "C");
        if (cliente == null) return Unauthorized();
        if (cliente.SenhaHash == null) return Conflict("Defina uma senha em Minha conta para confirmar o pedido.");

        if (dados.AtendimentoItens.Any(item => item is null))
            return BadRequest("O pedido contém uma linha inválida.");

        await using var transacao = await _context.Database.BeginTransactionAsync();
        var atendimento = new Atendimento
        {
            IdPessoa = cliente.Id, Observacao = dados.Observacao, Modalidade = dados.Modalidade,
            TaxaEntrega = dados.Modalidade == "entrega" ? 5m : 0m
        };
        foreach (var grupo in dados.AtendimentoItens.GroupBy(item => item.IdItem).OrderBy(g => g.Key))
        {
            var total = grupo.Sum(item => (long)item.Quantidade);
            if (total > int.MaxValue) return BadRequest("Quantidade inválida.");
            var quantidade = (int)total;
            var item = await _context.Itens.AsNoTracking()
                .SingleOrDefaultAsync(i => i.IdItem == grupo.Key && i.AtivoProdutoServico == "S");
            if (item == null || item.TipoItem != "P") return BadRequest("O pedido contém itens indisponíveis.");

            var pizza = item.EhPizza;
            if (grupo.Any(linha => pizza ? linha.Fatias is not (4 or 6 or 8) : linha.Fatias.HasValue))
                return BadRequest($"Confira o tamanho selecionado para {item.Nome}.");

            if (item.TipoItem == "P")
            {
                var linhas = await _context.Itens.Where(i => i.IdItem == item.IdItem &&
                        i.AtivoProdutoServico == "S" && i.Saldo >= quantidade)
                    .ExecuteUpdateAsync(update => update.SetProperty(i => i.Saldo, i => i.Saldo - quantidade));
                if (linhas == 0) return Conflict($"Estoque insuficiente para {item.Nome}.");
                atendimento.EstoqueBaixado = true;
            }
            foreach (var tamanho in grupo.GroupBy(linha => linha.Fatias))
            {
                atendimento.AtendimentoItens.Add(new AtendimentoItem
                {
                    IdItem = item.IdItem, Quantidade = tamanho.Sum(linha => linha.Quantidade),
                    PrecoUnitario = item.Preco, Fatias = tamanho.Key
                });
            }
        }

        _context.Atendimentos.Add(atendimento);
        await _context.SaveChangesAsync();
        await transacao.CommitAsync();
        return CreatedAtAction(nameof(GetPedido), new { id = atendimento.IdAtendimento }, atendimento);
    }

    [HttpPut("{id:int}/status")]
    public Task<IActionResult> AlterarStatus(int id, StatusDados dados) => AtualizarStatus(id, dados.Status);

    [HttpDelete("{id:int}")]
    public Task<IActionResult> Cancelar(int id) => AtualizarStatus(id, "Cancelado");

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> CancelarComoCliente(int id,
        [FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();
        var cliente = await _context.Pessoas.SingleOrDefaultAsync(p => p.TokenPedidos == token && p.Tipo == "C");
        if (cliente == null) return Unauthorized();
        return await AtualizarStatus(id, "Cancelado", cliente.Id);
    }

    private async Task<IActionResult> AtualizarStatus(int id, string status, int? clienteId = null)
    {
        await using var transacao = await _context.Database.BeginTransactionAsync();
        var pedido = await _context.Atendimentos.Include(p => p.AtendimentoItens)
            .SingleOrDefaultAsync(p => p.IdAtendimento == id && (!clienteId.HasValue || p.IdPessoa == clienteId));
        if (pedido == null) return NotFound();

        if (clienteId.HasValue && pedido.Status != "Em Processamento")
            return Conflict("O cancelamento pelo cliente só é permitido em processamento.");

        var ativos = new[] { "Em Processamento", "Sendo preparado", "Saiu para entrega", "Pronto para retirada" };
        var permitido = ativos.Contains(pedido.Status) && pedido.Status != status &&
            (ativos.Contains(status) || status is "Cancelado" or "Concluído");
        if (status == "Saiu para entrega" && pedido.Modalidade == "retirada") permitido = false;
        if (status == "Pronto para retirada" && pedido.Modalidade == "entrega") permitido = false;
        if (!permitido) return Conflict("Transição de situação inválida.");

        var atualizado = await _context.Atendimentos.Where(p => p.IdAtendimento == id && p.Status == pedido.Status)
            .ExecuteUpdateAsync(update => update.SetProperty(p => p.Status, status));
        if (atualizado == 0) return Conflict("A situação do pedido mudou. Atualize a lista.");

        // Pedidos antigos não tiveram baixa de estoque.
        if (status == "Cancelado" && pedido.EstoqueBaixado)
        {
            foreach (var item in pedido.AtendimentoItens.OrderBy(i => i.IdItem))
            {
                await _context.Itens.Where(i => i.IdItem == item.IdItem && i.TipoItem == "P")
                    .ExecuteUpdateAsync(update => update.SetProperty(i => i.Saldo, i => i.Saldo + item.Quantidade));
            }
        }
        await transacao.CommitAsync();
        return NoContent();
    }

    public record StatusDados(string Status);
}
