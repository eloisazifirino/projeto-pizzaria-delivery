using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;
using PizzariaApi.Models;

namespace PizzariaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItensController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItensController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetItens()
    {
        var itens = await _context.Itens.ToListAsync();
        return Ok(itens);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetItem(int id)
    {
        var item = await _context.Itens.FindAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public Task<IActionResult> Criar(ItemDados dados) => Salvar(null, dados);

    [HttpPut("{id:int}")]
    public Task<IActionResult> Editar(int id, ItemDados dados) => Salvar(id, dados);

    private async Task<IActionResult> Salvar(int? id, ItemDados dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome) || dados.Nome.Trim().Length > 100 ||
            dados.DescricaoItem?.Length > 200 || dados.Preco < 0 || dados.Preco > 99999999.99m ||
            dados.TipoItem != "P" || dados.GrupoCardapio is not ("pizza" or "bebidas" or "outros") ||
            dados.Detalhes?.Length > 2000)
            return BadRequest("Confira nome, descrição, preço e tipo do item.");
        if (dados.IdCategoria.HasValue && !await _context.Categorias.AnyAsync(c => c.Id == dados.IdCategoria.Value && c.GrupoCardapio == (dados.TipoItem == "S" ? "servicos" : dados.GrupoCardapio)))
            return BadRequest("A categoria deve pertencer à aba selecionada.");

        var item = id.HasValue ? await _context.Itens.FindAsync(id.Value) : new Item();
        if (item == null) return NotFound();
        if (id.HasValue && item.TipoItem != dados.TipoItem)
            return BadRequest("O tipo de um item cadastrado é fixo.");

        item.Nome = dados.Nome.Trim();
        item.IdCategoria = dados.IdCategoria;
        item.DescricaoItem = dados.DescricaoItem;
        item.Preco = dados.Preco;
        item.GrupoCardapio = dados.TipoItem == "S" ? "outros" : dados.GrupoCardapio;
        item.Detalhes = dados.Detalhes?.Trim();
        item.TipoItem = dados.TipoItem;
        if (!id.HasValue) _context.Itens.Add(item);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desativar(int id)
    {
        var item = await _context.Itens.FindAsync(id);
        if (item == null) return NotFound();
        item.AtivoProdutoServico = "N";
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id)
    {
        var item = await _context.Itens.FindAsync(id);
        if (item == null) return NotFound();
        if (item.TipoItem != "P") return BadRequest("Somente produtos podem ser reativados.");
        item.AtivoProdutoServico = "S";
        await _context.SaveChangesAsync();
        return NoContent();
    }

    public record ItemDados(string Nome, int? IdCategoria, string? DescricaoItem, decimal Preco, string TipoItem,
        string GrupoCardapio = "outros", string? Detalhes = null);
}
