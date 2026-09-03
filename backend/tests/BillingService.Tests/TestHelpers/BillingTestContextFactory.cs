using KorpTeste.BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Tests.TestHelpers;

internal static class BillingTestContextFactory
{
    public static BillingDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BillingDbContext(options);
    }
}
