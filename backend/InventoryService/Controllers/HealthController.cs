using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KorpTeste.InventoryService.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(HealthCheckService healthCheckService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);
        var resposta = new
        {
            status = TraduzirStatus(report.Status),
            verificacoes = report.Entries.Select(item => new
            {
                nome = item.Key,
                status = TraduzirStatus(item.Value.Status),
                mensagem = item.Value.Description
            })
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(resposta)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, resposta);
    }

    private static string TraduzirStatus(HealthStatus status) => status switch
    {
        HealthStatus.Healthy => "Saudavel",
        HealthStatus.Degraded => "Degradado",
        HealthStatus.Unhealthy => "Indisponivel",
        _ => "Desconhecido"
    };
}

