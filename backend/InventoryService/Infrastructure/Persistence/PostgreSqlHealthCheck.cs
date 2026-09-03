using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KorpTeste.InventoryService.Infrastructure.Persistence;

public sealed class PostgreSqlHealthCheck(InventoryDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var conectado = await dbContext.Database.CanConnectAsync(cancellationToken);

        return conectado
            ? HealthCheckResult.Healthy("Banco PostgreSQL acessível.")
            : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco PostgreSQL do serviço de estoque.");
    }
}

