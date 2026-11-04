using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PizzariaApi.Data;
using PizzariaApi.Models;

namespace PizzariaApi.Controllers;

[ApiController, Route("api/pessoas")]
public class PessoasController(AppDbContext db, IPasswordHasher<Pessoa> senhas) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await db.Pessoas.Where(p => p.Tipo == "C")
        .Select(p => new { p.Id, p.Nome, p.Email, p.Cpf, p.Telefone }).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Consultar(int id) =>
        await db.Pessoas.FindAsync(id) is { Tipo: "C" } p
            ? Ok(new { p.Id, p.Nome, p.Email, p.Cpf, p.Telefone }) : NotFound();

    [HttpPost]
    public async Task<IActionResult> Cadastrar(Cadastro dados)
    {
        var pessoa = new Pessoa
        {
            Nome = dados.Nome.Trim(), Cpf = dados.Cpf,
            Email = dados.Email.Trim().ToLowerInvariant(), Telefone = dados.Telefone,
            TokenPedidos = Convert.ToHexString(RandomNumberGenerator.GetBytes(32))
        };
        pessoa.SenhaHash = senhas.HashPassword(pessoa, dados.Senha);
        db.Pessoas.Add(pessoa);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException { Number: 1062 })
        {
            return Conflict("CPF ou e-mail já cadastrado.");
        }
        return CreatedAtAction(nameof(Consultar), new { id = pessoa.Id },
            Sessao(pessoa));
    }

    [HttpPost("entrar")]
    public async Task<IActionResult> Entrar(Credenciais dados)
    {
        var email = dados.Email.Trim().ToLowerInvariant();
        var pessoa = await db.Pessoas.SingleOrDefaultAsync(p => p.Email == email && p.Tipo == "C");
        if (pessoa?.SenhaHash == null ||
            senhas.VerifyHashedPassword(pessoa, pessoa.SenhaHash, dados.Senha) == PasswordVerificationResult.Failed)
            return Unauthorized("E-mail ou senha inválidos.");

        pessoa.TokenPedidos = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await db.SaveChangesAsync();
        return Ok(Sessao(pessoa));
    }

    [HttpPost("definir-senha")]
    public async Task<IActionResult> DefinirSenha(NovaSenha dados,
        [FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();
        var pessoa = await db.Pessoas.SingleOrDefaultAsync(p => p.TokenPedidos == token && p.Tipo == "C");
        if (pessoa == null) return Unauthorized();

        var hash = senhas.HashPassword(pessoa, dados.Senha);
        var alterados = await db.Pessoas.Where(p => p.Id == pessoa.Id && p.TokenPedidos == token && p.SenhaHash == null)
            .ExecuteUpdateAsync(update => update.SetProperty(p => p.SenhaHash, hash));
        if (alterados == 0) return Conflict("A conta já tem uma senha. Entre com seu e-mail e senha.");
        pessoa.SenhaHash = hash;
        return Ok(Sessao(pessoa));
    }

    [HttpPost("sair")]
    public async Task<IActionResult> Sair([FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        if (!string.IsNullOrEmpty(token))
            await db.Pessoas.Where(p => p.TokenPedidos == token)
                .ExecuteUpdateAsync(update => update.SetProperty(p => p.TokenPedidos, (string?)null));
        return NoContent();
    }

    private static object Sessao(Pessoa pessoa) =>
        new { pessoa.Id, pessoa.Nome, Token = pessoa.TokenPedidos, TemSenha = pessoa.SenhaHash != null };

    public record Credenciais(
        [Required, EmailAddress, StringLength(254)] string Email,
        [Required, StringLength(128)] string Senha);

    public record NovaSenha([Required, StringLength(128, MinimumLength = 8)] string Senha);

    [HttpGet("perfil")]
    public async Task<IActionResult> ConsultarPerfil([FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        var pessoa = await ClienteDaSessao(token);
        return pessoa == null ? Unauthorized() : Ok(Perfil(pessoa));
    }

    [HttpPut("perfil")]
    public async Task<IActionResult> EditarPerfil(DadosPerfil dados,
        [FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        var pessoa = await ClienteDaSessao(token);
        if (pessoa == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dados.Nome)) return BadRequest("Informe seu nome.");

        pessoa.Nome = dados.Nome.Trim();
        pessoa.Cpf = dados.Cpf;
        pessoa.Email = dados.Email.Trim().ToLowerInvariant();
        pessoa.Telefone = dados.Telefone;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException { Number: 1062 })
        {
            return Conflict("CPF ou e-mail já cadastrado.");
        }
        return Ok(Perfil(pessoa));
    }

    private Task<Pessoa?> ClienteDaSessao(string? token) => string.IsNullOrEmpty(token)
        ? Task.FromResult<Pessoa?>(null)
        : db.Pessoas.SingleOrDefaultAsync(p => p.TokenPedidos == token && p.Tipo == "C");

    private static object Perfil(Pessoa pessoa) =>
        new { pessoa.Id, pessoa.Nome, pessoa.Cpf, pessoa.Email, pessoa.Telefone };

    public record DadosPerfil(
        [Required, StringLength(100)] string Nome,
        [Required, RegularExpression("^[0-9]{11}$")] string Cpf,
        [Required, EmailAddress, StringLength(254)] string Email,
        [RegularExpression("^[0-9]{10,11}$")] string? Telefone);

    [HttpGet("meus-pedidos")]
    public async Task<IActionResult> MeusPedidos([FromHeader(Name = "X-Cliente-Token")] string? token)
    {
        if (string.IsNullOrEmpty(token)) return Unauthorized();
        var pessoa = await db.Pessoas.SingleOrDefaultAsync(p => p.TokenPedidos == token && p.Tipo == "C");
        if (pessoa == null) return Unauthorized();
        return Ok(await db.Atendimentos.Where(p => p.IdPessoa == pessoa.Id)
            .Include(p => p.AtendimentoItens).OrderByDescending(p => p.IdAtendimento).ToListAsync());
    }

    public record Cadastro(
        [Required, StringLength(100)] string Nome,
        [Required, RegularExpression("^[0-9]{11}$")] string Cpf,
        [Required, EmailAddress, StringLength(254)] string Email,
        [RegularExpression("^[0-9]{10,11}$")] string? Telefone,
        [Required, StringLength(128, MinimumLength = 8)] string Senha);
}
