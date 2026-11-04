using Microsoft.EntityFrameworkCore;
using PizzariaApi.Models;

namespace PizzariaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Itens { get; set; }
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Pessoa> Pessoas => Set<Pessoa>();
    public DbSet<EntradaProduto> Entradas => Set<EntradaProduto>();
    public DbSet<Atendimento> Atendimentos { get; set; }
    public DbSet<AtendimentoItem> AtendimentoItens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Atendimento>()
            .HasMany(pedido => pedido.AtendimentoItens)
            .WithOne()
            .HasForeignKey(item => item.IdAtendimento);
    }
}