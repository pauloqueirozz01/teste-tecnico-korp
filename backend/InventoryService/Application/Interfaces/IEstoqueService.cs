using KorpTeste.InventoryService.Application.DTOs;

namespace KorpTeste.InventoryService.Application.Interfaces;

public interface IEstoqueService
{
    Task<ValidarEstoqueResponse> ValidarAsync(ValidarEstoqueRequest request, CancellationToken cancellationToken);
    Task<ConsumirEstoqueResponse> ConsumirAsync(ConsumirEstoqueRequest request, string? idempotencyKey, CancellationToken cancellationToken);
    Task<ReporEstoqueResponse> ReporAsync(ReporEstoqueRequest request, string? idempotencyKey, CancellationToken cancellationToken);
}
