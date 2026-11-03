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
    public async Task<IActionResult> GetPedidos()
    {
        var pedidos = await _context.Atendimentos
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
    public async Task<IActionResult> CriarPedido(CriarPedidoDto dados)
    {
        var ids = dados.AtendimentoItens.Select(item => item.IdItem).Distinct().ToArray();
        var precos = await _context.Itens
            .Where(item => ids.Contains(item.IdItem) && item.AtivoProdutoServico == "S")
            .ToDictionaryAsync(item => item.IdItem, item => item.Preco);

        if (precos.Count != ids.Length)
            return BadRequest("O pedido contém itens indisponíveis.");

        var atendimento = new Atendimento
        {
            Observacao = dados.Observacao,
            AtendimentoItens = dados.AtendimentoItens.Select(item => new AtendimentoItem
            {
                IdItem = item.IdItem,
                Quantidade = item.Quantidade,
                PrecoUnitario = precos[item.IdItem]
            }).ToList()
        };

        _context.Atendimentos.Add(atendimento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = atendimento.IdAtendimento }, atendimento);
    }

    // DELETE: Cancelar/Remover pedido
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelarPedido(int id)
    {
        var pedido = await _context.Atendimentos
            .Include(a => a.AtendimentoItens)
            .FirstOrDefaultAsync(a => a.IdAtendimento == id);
        if (pedido == null) return NotFound();

        _context.Atendimentos.Remove(pedido);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}