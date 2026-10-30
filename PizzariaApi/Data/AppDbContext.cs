using Microsoft.EntityFrameworkCore;
using PizzariaApi.Models;

namespace PizzariaApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Itens { get; set; }
}