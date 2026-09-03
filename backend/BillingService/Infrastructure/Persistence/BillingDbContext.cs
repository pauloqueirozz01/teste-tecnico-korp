using Microsoft.EntityFrameworkCore;

namespace KorpTeste.BillingService.Infrastructure.Persistence;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entities.NotaFiscal> NotasFiscais => Set<Domain.Entities.NotaFiscal>();

    public DbSet<Domain.Entities.ItemNotaFiscal> ItensNotaFiscal => Set<Domain.Entities.ItemNotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("nota_fiscal_numero_seq")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
    }
}
