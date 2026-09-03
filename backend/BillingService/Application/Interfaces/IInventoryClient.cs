using KorpTeste.BillingService.Application.DTOs;

namespace KorpTeste.BillingService.Application.Interfaces;

public interface IInventoryClient
{
    Task<ResultadoConsumoEstoqueResponse> ConsumirAsync(
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<ResultadoConsumoEstoqueResponse> ReporAsync(
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken);
}
