using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;
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

    // CREATE: Registrar novo pedido
    [HttpPost]
    public async Task<IActionResult> CriarPedido([FromBody] Atendimento atendimento)
    {
        atendimento.DataHora = DateTime.Now;
        atendimento.Status = "Em Processamento";

        _context.Atendimentos.Add(atendimento);
        await _context.SaveChangesAsync();

        return Ok(atendimento);
    }

    // DELETE: Cancelar/Remover pedido
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelarPedido(int id)
    {
        var pedido = await _context.Atendimentos.FindAsync(id);
        if (pedido == null) return NotFound();

        _context.Atendimentos.Remove(pedido);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}