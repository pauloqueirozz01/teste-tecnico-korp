using BillingService.Tests.TestHelpers;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Infrastructure.Persistence;
using KorpTeste.BillingService.Infrastructure.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BillingService.Tests.TestHelpers;

public sealed class BillingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    internal InventoryClientFake InventoryClient { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BillingDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<BillingDbContext>>();
            services.RemoveAll<INumeradorNotaFiscal>();
            services.RemoveAll<IInventoryClient>();

            services.AddDbContext<BillingDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
            services.AddSingleton<INumeradorNotaFiscal, SequencialNotaFiscalFake>();
            services.AddSingleton<IInventoryClient>(InventoryClient);
            services.Configure<ArmazenamentoNotasOptions>(options =>
                options.Diretorio = $"storage/testes/{_databaseName}");
        });
    }
}
