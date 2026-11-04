using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;
using PizzariaApi.Models;

namespace PizzariaApi.Controllers;

[ApiController, Route("api/estoque")]
public class EstoqueController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await db.Itens.Where(i => i.TipoItem == "P").ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Consultar(int id) =>
        await db.Itens.SingleOrDefaultAsync(i => i.IdItem == id && i.TipoItem == "P") is { } item
            ? Ok(item) : NotFound();

    [HttpGet("entradas")]
    public async Task<IActionResult> Entradas() => Ok(await db.Entradas.OrderByDescending(e => e.Id).ToListAsync());

    [HttpPost("entradas")]
    public async Task<IActionResult> Registrar(EntradaDados dados)
    {
        if (dados.Data == default) return BadRequest("Informe a data de entrada.");
        if (dados.IdItem is null && (string.IsNullOrWhiteSpace(dados.Nome) ||
            dados.Nome.Trim().Length > 100))
            return BadRequest("Informe o nome do produto.");

        await using var transacao = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            return await RegistrarEntrada(dados, transacao);
        }
        catch (Exception erro) when (erro.GetBaseException() is MySqlConnector.MySqlException { Number: 1205 or 1213 })
        {
            return Conflict("Outra movimentação ocorreu ao mesmo tempo. Tente novamente.");
        }
    }

    private async Task<IActionResult> RegistrarEntrada(EntradaDados dados,
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transacao)
    {
        var produtos = db.Itens.Where(i => i.TipoItem == "P");
        var nome = dados.Nome?.Trim();
        var candidatos = await (dados.IdItem.HasValue
            ? produtos.Where(i => i.IdItem == dados.IdItem.Value)
            : produtos.Where(i => i.Nome == nome)).Take(2).ToListAsync();

        if (candidatos.Count > 1) return Conflict("Há produtos com esse nome. Selecione o produto na lista.");
        if (dados.IdItem.HasValue && candidatos.Count == 0) return NotFound("Produto inexistente.");

        var produto = candidatos.SingleOrDefault();
        if (produto == null)
        {
            if (dados.ValorUnitario is null or < 0 or > 99999999.99m)
                return BadRequest("Informe um valor unitário de entrada válido.");
            if (dados.Preco is null or < 0 or > 99999999.99m ||
                dados.GrupoCardapio is not ("pizza" or "bebidas" or "outros") || dados.DescricaoItem?.Length > 200 || dados.Detalhes?.Length > 2000)
                return BadRequest("Confira o preço de venda, a aba do cardápio e a descrição.");
            if (dados.IdCategoria.HasValue && !await db.Categorias.AnyAsync(c => c.Id == dados.IdCategoria.Value && c.GrupoCardapio == dados.GrupoCardapio))
                return BadRequest("A categoria deve pertencer à aba selecionada.");

            produto = new Item
            {
                Nome = nome!, IdCategoria = dados.IdCategoria, Preco = dados.Preco.Value,
                GrupoCardapio = dados.GrupoCardapio, DescricaoItem = dados.DescricaoItem,
                Detalhes = dados.Detalhes?.Trim(), Saldo = dados.Quantidade
            };
            db.Itens.Add(produto);
            await db.SaveChangesAsync();
        }
        else
        {
            var linhas = await produtos.Where(i => i.IdItem == produto.IdItem &&
                    i.Saldo <= int.MaxValue - dados.Quantidade)
                .ExecuteUpdateAsync(update => update.SetProperty(i => i.Saldo, i => i.Saldo + dados.Quantidade));
            if (linhas == 0) return Conflict("O saldo ultrapassaria o limite permitido.");
        }

        var entrada = new EntradaProduto
        {
            IdItem = produto.IdItem, Data = dados.Data, Quantidade = dados.Quantidade,
            ValorUnitario = candidatos.Count == 0 ? dados.ValorUnitario!.Value : produto.Preco
        };
        db.Entradas.Add(entrada);
        await db.SaveChangesAsync();
        var saldo = await produtos.Where(i => i.IdItem == produto.IdItem).Select(i => i.Saldo).SingleAsync();
        await transacao.CommitAsync();
        return Ok(new { entrada.Id, entrada.IdItem, entrada.Quantidade, entrada.Data, entrada.ValorUnitario,
            Saldo = saldo, NovoProduto = candidatos.Count == 0 });
    }

    [HttpPost("saidas")]
    public async Task<IActionResult> Retirar(SaidaDados dados)
    {
        if (dados.Data == default) return BadRequest("Informe a data da retirada.");
        await using var transacao = await db.Database.BeginTransactionAsync();
        var produto = db.Itens.Where(i => i.IdItem == dados.IdItem && i.TipoItem == "P");
        if (!await produto.AnyAsync()) return NotFound("Produto inexistente.");

        var linhas = await produto.Where(i => i.Saldo >= dados.Quantidade)
            .ExecuteUpdateAsync(update => update.SetProperty(i => i.Saldo, i => i.Saldo - dados.Quantidade));
        if (linhas == 0) return Conflict("A retirada é maior que o saldo em estoque.");

        db.Entradas.Add(new EntradaProduto
        {
            IdItem = dados.IdItem, Data = dados.Data, Quantidade = -dados.Quantidade, ValorUnitario = 0
        });
        await db.SaveChangesAsync();
        await transacao.CommitAsync();
        return NoContent();
    }

    public record SaidaDados(
        [Range(1, int.MaxValue)] int IdItem,
        [Range(1, 1000000)] int Quantidade,
        DateOnly Data);

    public record EntradaDados(
        [Range(1, int.MaxValue)] int? IdItem,
        [Range(1, 1000000)] int Quantidade,
        DateOnly Data,
        decimal? ValorUnitario = null,
        string? Nome = null, decimal? Preco = null, int? IdCategoria = null,
        string GrupoCardapio = "outros", string? DescricaoItem = null, string? Detalhes = null);
}
