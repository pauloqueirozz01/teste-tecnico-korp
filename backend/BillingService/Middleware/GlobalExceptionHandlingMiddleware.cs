using System.Net;
using System.Text.Json;
using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Domain.Exceptions;

namespace KorpTeste.BillingService.Middleware;

public sealed class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BillingDomainException exception)
        {
            await EscreverErroAsync(context, exception.Codigo, exception.Message, exception.StatusCode);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Erro não tratado durante o processamento da requisição.");
            await EscreverErroAsync(
                context,
                "ERRO_INTERNO",
                "Ocorreu um erro interno ao processar a requisição.",
                (int)HttpStatusCode.InternalServerError);
        }
    }

    private static async Task EscreverErroAsync(
        HttpContext context,
        string codigo,
        string mensagem,
        int statusCode)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var resposta = new ErroHttpResponse(codigo, mensagem, statusCode);

        await context.Response.WriteAsync(JsonSerializer.Serialize(resposta));
    }
}
