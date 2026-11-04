using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;
using PizzariaApi.Models;

namespace PizzariaApi.Controllers;

[ApiController, Route("api/categorias")]
public class CategoriasController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await db.Categorias.OrderBy(c => c.Nome).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Consultar(int id) =>
        await db.Categorias.FindAsync(id) is { } categoria ? Ok(categoria) : NotFound();

    [HttpPost]
    public Task<IActionResult> Criar(NomeCategoria dados) => Salvar(null, dados);

    [HttpPut("{id:int}")]
    public Task<IActionResult> Editar(int id, NomeCategoria dados) => Salvar(id, dados);

    private async Task<IActionResult> Salvar(int? id, NomeCategoria dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome) || dados.Nome.Trim().Length > 45)
            return BadRequest("Informe um nome de categoria de até 45 caracteres.");

        if (dados.GrupoCardapio is not ("pizza" or "bebidas" or "outros"))
            return BadRequest("Selecione a aba da categoria.");

        var categoria = id.HasValue ? await db.Categorias.FindAsync(id.Value) : new Categoria();
        if (categoria == null) return NotFound();
        if (id.HasValue && categoria.GrupoCardapio != dados.GrupoCardapio &&
            await db.Itens.AnyAsync(i => i.IdCategoria == id.Value))
            return Conflict("A categoria tem itens vinculados. Crie uma categoria para a outra aba.");
        categoria.GrupoCardapio = dados.GrupoCardapio;
        categoria.Nome = dados.Nome.Trim();
        if (!id.HasValue) db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);
        if (categoria == null) return NotFound();

        db.Categorias.Remove(categoria);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException { Number: 1451 })
        {
            return Conflict("A categoria está vinculada a itens do catálogo.");
        }
        return NoContent();
    }

    public record NomeCategoria(string Nome, string GrupoCardapio = "outros");
}
