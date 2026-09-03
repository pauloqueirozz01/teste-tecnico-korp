using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BillingService.Tests.TestHelpers;

internal sealed class InventoryClientFake : IInventoryClient
{
    public bool Indisponivel { get; set; }
    public bool SaldoInsuficiente { get; set; }
    public List<ConsumirEstoqueRequest> Consumos { get; } = [];
    public List<ConsumirEstoqueRequest> Reposicoes { get; } = [];
    public bool FalharAoRepor { get; set; }

    public Task<ResultadoConsumoEstoqueResponse> ConsumirAsync(
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (Indisponivel)
        {
            throw new InventoryServiceIndisponivelException();
        }

        if (SaldoInsuficiente)
        {
            throw new InventoryServiceRespostaException(
                "SALDO_INSUFICIENTE",
                "O produto não possui saldo suficiente para esta operação.",
                StatusCodes.Status409Conflict);
        }

        Consumos.Add(request);
        return Task.FromResult(new ResultadoConsumoEstoqueResponse("Estoque consumido com sucesso."));
    }

    public Task<ResultadoConsumoEstoqueResponse> ReporAsync(
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (FalharAoRepor)
        {
            throw new InventoryServiceIndisponivelException();
        }

        Reposicoes.Add(request);
        return Task.FromResult(new ResultadoConsumoEstoqueResponse("Estoque reposto com sucesso."));
    }
}
