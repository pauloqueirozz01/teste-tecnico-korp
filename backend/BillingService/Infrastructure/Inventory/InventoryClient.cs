using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Polly.CircuitBreaker;
using Polly.Timeout;
using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Domain.Exceptions;

namespace KorpTeste.BillingService.Infrastructure.Inventory;

public sealed class InventoryClient(
    HttpClient httpClient,
    ILogger<InventoryClient> logger) : IInventoryClient
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";

    public Task<ResultadoConsumoEstoqueResponse> ConsumirAsync(
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        return EnviarAsync("api/estoque/consumir", request, idempotencyKey, cancellationToken);
    }

    public Task<ResultadoConsumoEstoqueResponse> ReporAsync(
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        return EnviarAsync("api/estoque/repor", request, idempotencyKey, cancellationToken);
    }

    private async Task<ResultadoConsumoEstoqueResponse> EnviarAsync(
        string rota,
        ConsumirEstoqueRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage resposta;

        try
        {
            using var mensagem = new HttpRequestMessage(HttpMethod.Post, rota)
            {
                Content = JsonContent.Create(request)
            };
            mensagem.Headers.TryAddWithoutValidation(IdempotencyKeyHeader, idempotencyKey);

            resposta = await httpClient.SendAsync(mensagem, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Falha de comunicação com o InventoryService na rota {Rota}.", rota);
            throw new InventoryServiceIndisponivelException(exception);
        }
        catch (BrokenCircuitException exception)
        {
            logger.LogWarning(exception, "Circuit breaker aberto para comunicação com o InventoryService na rota {Rota}.", rota);
            throw new InventoryServiceIndisponivelException(exception);
        }
        catch (TimeoutRejectedException exception)
        {
            logger.LogWarning(exception, "Timeout controlado pela policy de resiliência na rota {Rota}.", rota);
            throw new InventoryServiceIndisponivelException(exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Timeout de comunicação com o InventoryService na rota {Rota}.", rota);
            throw new InventoryServiceIndisponivelException(exception);
        }

        using (resposta)
        {
            if (resposta.IsSuccessStatusCode)
            {
                var sucesso = await resposta.Content.ReadFromJsonAsync<ResultadoConsumoEstoqueResponse>(cancellationToken);
                return sucesso ?? new ResultadoConsumoEstoqueResponse("Operação de estoque concluída.");
            }

            var erro = await LerErroAsync(resposta, cancellationToken);

            if (resposta.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InventoryServiceRespostaException(
                    erro?.Codigo ?? "SALDO_INSUFICIENTE",
                    erro?.Mensagem ?? "O estoque não está disponível para esta operação.",
                    StatusCodes.Status409Conflict);
            }

            if (resposta.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InventoryServiceRespostaException(
                    erro?.Codigo ?? "PRODUTO_NAO_ENCONTRADO",
                    erro?.Mensagem ?? "Um ou mais produtos da nota fiscal não foram encontrados no estoque.",
                    StatusCodes.Status404NotFound);
            }

            if (resposta.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new InventoryServiceRespostaException(
                    erro?.Codigo ?? "REQUISICAO_INVALIDA",
                    erro?.Mensagem ?? "A requisição de estoque é inválida.",
                    StatusCodes.Status400BadRequest);
            }

            if ((int)resposta.StatusCode >= 500 || resposta.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new InventoryServiceIndisponivelException();
            }

            throw new InventoryServiceRespostaException(
                erro?.Codigo ?? "FALHA_PROCESSAMENTO_NOTA",
                erro?.Mensagem ?? "O serviço de estoque não concluiu a operação.",
                (int)resposta.StatusCode);
        }
    }

    private static async Task<InventoryErroResponse?> LerErroAsync(
        HttpResponseMessage resposta,
        CancellationToken cancellationToken)
    {
        try
        {
            return await resposta.Content.ReadFromJsonAsync<InventoryErroResponse>(cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
