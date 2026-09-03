using Microsoft.EntityFrameworkCore;
using KorpTeste.InventoryService.Domain.Entities;

namespace KorpTeste.InventoryService.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<OperacaoEstoqueIdempotente> OperacoesEstoqueIdempotentes => Set<OperacaoEstoqueIdempotente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
