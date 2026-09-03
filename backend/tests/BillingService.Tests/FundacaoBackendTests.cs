using KorpTeste.BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Tests;

public class FundacaoBackendTests
{
    [Fact]
    public void BillingDbContext_DeveUsarProviderPostgreSql()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseNpgsql("Host=localhost;Database=korp_billing;Username=korp_dev;Password=korp_dev_password")
            .Options;

        using var context = new BillingDbContext(options);

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", context.Database.ProviderName);
    }
}

