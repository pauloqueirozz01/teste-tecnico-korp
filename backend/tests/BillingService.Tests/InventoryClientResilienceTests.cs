using System.Net;
using System.Net.Http.Json;
using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Domain.Exceptions;
using KorpTeste.BillingService.Infrastructure.Inventory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace BillingService.Tests;

public sealed class InventoryClientResilienceTests
{
    [Fact]
    public async Task ConsumirAsync_DeveRepetirFalhaTransitoriaERetornarSucesso()
    {
        var handler = new SequencedHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            CriarSucesso());
        var client = CriarClient(handler, retryCount: 2);

        await client.ConsumirAsync(CriarRequest(), "consumo-teste-retry", CancellationToken.None);

        Assert.Equal(2, handler.Chamadas);
    }

    [Fact]
    public async Task ConsumirAsync_NaoDeveRepetirErroDeNegocio()
    {
        var handler = new SequencedHandler(new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = JsonContent.Create(new InventoryErroResponse(
                "SALDO_INSUFICIENTE",
                "O produto não possui saldo suficiente para concluir a operação.",
                StatusCodes.Status409Conflict))
        });
        var client = CriarClient(handler, retryCount: 2);

        await Assert.ThrowsAsync<InventoryServiceRespostaException>(() =>
            client.ConsumirAsync(CriarRequest(), "consumo-teste-negocio", CancellationToken.None));

        Assert.Equal(1, handler.Chamadas);
    }

    [Fact]
    public async Task ConsumirAsync_DeveAbrirCircuitBreakerAposFalhasTransitorias()
    {
        var handler = new SequencedHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            CriarSucesso());
        var client = CriarClient(handler, retryCount: 0, circuitBreakMilliseconds: 500);

        await Assert.ThrowsAsync<InventoryServiceIndisponivelException>(() =>
            client.ConsumirAsync(CriarRequest(), "consumo-circuito-1", CancellationToken.None));
        await Assert.ThrowsAsync<InventoryServiceIndisponivelException>(() =>
            client.ConsumirAsync(CriarRequest(), "consumo-circuito-2", CancellationToken.None));
        await Assert.ThrowsAsync<InventoryServiceIndisponivelException>(() =>
            client.ConsumirAsync(CriarRequest(), "consumo-circuito-3", CancellationToken.None));

        Assert.Equal(2, handler.Chamadas);
    }

    [Fact]
    public async Task ConsumirAsync_DeveRecuperarCircuitBreakerAposIntervalo()
    {
        var handler = new SequencedHandler(
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
            CriarSucesso());
        var client = CriarClient(handler, retryCount: 0, circuitBreakMilliseconds: 500);

        await Assert.ThrowsAsync<InventoryServiceIndisponivelException>(() =>
            client.ConsumirAsync(CriarRequest(), "consumo-recuperacao-1", CancellationToken.None));
        await Assert.ThrowsAsync<InventoryServiceIndisponivelException>(() =>
            client.ConsumirAsync(CriarRequest(), "consumo-recuperacao-2", CancellationToken.None));
        await Task.Delay(650);

        await client.ConsumirAsync(CriarRequest(), "consumo-recuperacao-3", CancellationToken.None);

        Assert.Equal(3, handler.Chamadas);
    }

    private static IInventoryClient CriarClient(
        SequencedHandler handler,
        int retryCount,
        int circuitBreakMilliseconds = 500)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
        {
            client.BaseAddress = new Uri("http://inventory-service");
            client.Timeout = Timeout.InfiniteTimeSpan;
        })
        .ConfigurePrimaryHttpMessageHandler(() => handler)
        .AddResilienceHandler("inventory-service-testes", pipeline =>
        {
            if (retryCount > 0)
            {
                pipeline.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = retryCount,
                    Delay = TimeSpan.FromMilliseconds(1),
                    ShouldHandle = static args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome))
                });
            }

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 1,
                MinimumThroughput = 2,
                SamplingDuration = TimeSpan.FromSeconds(5),
                BreakDuration = TimeSpan.FromMilliseconds(circuitBreakMilliseconds),
                ShouldHandle = static args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome))
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(1));
        });

        return services.BuildServiceProvider().GetRequiredService<IInventoryClient>();
    }

    private static ConsumirEstoqueRequest CriarRequest()
    {
        return new ConsumirEstoqueRequest([new ItemConsumoEstoqueRequest(Guid.NewGuid(), 1)]);
    }

    private static HttpResponseMessage CriarSucesso()
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new ResultadoConsumoEstoqueResponse("ok"))
        };
    }

    private sealed class SequencedHandler(params HttpResponseMessage[] responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new(responses);

        public int Chamadas { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Chamadas++;
            var response = _responses.Count > 0
                ? _responses.Dequeue()
                : CriarSucesso();

            return Task.FromResult(response);
        }
    }
}
