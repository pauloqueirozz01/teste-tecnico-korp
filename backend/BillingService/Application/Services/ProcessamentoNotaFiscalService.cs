using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Domain.Entities;
using KorpTeste.BillingService.Domain.Enums;
using KorpTeste.BillingService.Domain.Exceptions;
using KorpTeste.BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KorpTeste.BillingService.Application.Services;

public sealed class ProcessamentoNotaFiscalService(
    BillingDbContext context,
    IInventoryClient inventoryClient,
    IGeradorArquivoNotaFiscal geradorArquivo,
    ILogger<ProcessamentoNotaFiscalService> logger) : IProcessamentoNotaFiscalService
{
    public async Task<ResultadoProcessamentoNotaFiscalResponse> ProcessarAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notaFiscal = await context.NotasFiscais
            .Include(nota => nota.Itens)
            .SingleOrDefaultAsync(nota => nota.Id == id, cancellationToken);

        if (notaFiscal is null)
        {
            throw new NotaFiscalNaoEncontradaException(id);
        }

        try
        {
            notaFiscal.IniciarProcessamento();
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogInformation(exception, "A nota {NotaFiscalId} já está sendo processada por outra requisição.", id);
            throw new NotaFiscalEmProcessamentoException();
        }

        ArquivoNotaFiscalTemporario? arquivo = null;
        var estoqueConsumido = false;

        try
        {
            logger.LogInformation("Iniciando processamento da nota {Numero}.", notaFiscal.Numero);

            arquivo = await geradorArquivo.GerarTemporarioAsync(notaFiscal, cancellationToken);

            var requestEstoque = new ConsumirEstoqueRequest(
                notaFiscal.Itens
                    .Select(item => new ItemConsumoEstoqueRequest(item.ProdutoId, item.Quantidade))
                    .ToArray());

            logger.LogInformation("Solicitando consumo de estoque para a nota {Numero}.", notaFiscal.Numero);
            await inventoryClient.ConsumirAsync(
                requestEstoque,
                CriarChaveIdempotencia("consumo", notaFiscal.Id),
                cancellationToken);
            estoqueConsumido = true;

            await geradorArquivo.FinalizarAsync(arquivo, cancellationToken);

            var geradaEm = DateTimeOffset.UtcNow;
            notaFiscal.Fechar(arquivo.NomeArquivo, arquivo.CaminhoRelativo, geradaEm);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Nota {Numero} fechada com arquivo {NomeArquivo}.", notaFiscal.Numero, arquivo.NomeArquivo);
            return new ResultadoProcessamentoNotaFiscalResponse(MapearDetalhe(notaFiscal), arquivo.NomeArquivo);
        }
        catch (Exception exception)
        {
            var falhaCompensacao = await TratarFalhaAsync(notaFiscal, arquivo, estoqueConsumido, exception, cancellationToken);
            if (falhaCompensacao is not null)
            {
                throw falhaCompensacao;
            }

            throw;
        }
    }

    public async Task<ArquivoNotaFiscalDownload> ObterArquivoAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notaFiscal = await context.NotasFiscais
            .AsNoTracking()
            .SingleOrDefaultAsync(nota => nota.Id == id, cancellationToken);

        if (notaFiscal is null)
        {
            throw new NotaFiscalNaoEncontradaException(id);
        }

        if (notaFiscal.Status != StatusNotaFiscal.Fechada)
        {
            throw new NotaFiscalNaoProcessadaException();
        }

        return geradorArquivo.ObterDownload(notaFiscal);
    }

    private async Task<FalhaCompensacaoEstoqueException?> TratarFalhaAsync(
        NotaFiscal notaFiscal,
        ArquivoNotaFiscalTemporario? arquivo,
        bool estoqueConsumido,
        Exception exception,
        CancellationToken cancellationToken)
    {
        FalhaCompensacaoEstoqueException? falhaCompensacao = null;

        if (arquivo is not null)
        {
            await geradorArquivo.ExcluirAsync(arquivo, cancellationToken);

            if (estoqueConsumido)
            {
                await geradorArquivo.ExcluirDefinitivoAsync(arquivo, cancellationToken);
            }
        }

        if (estoqueConsumido)
        {
            try
            {
                var requestEstoque = new ConsumirEstoqueRequest(
                    notaFiscal.Itens
                        .Select(item => new ItemConsumoEstoqueRequest(item.ProdutoId, item.Quantidade))
                        .ToArray());

                logger.LogWarning("Tentando compensar consumo de estoque da nota {Numero}.", notaFiscal.Numero);
                await inventoryClient.ReporAsync(
                    requestEstoque,
                    CriarChaveIdempotencia("compensacao", notaFiscal.Id),
                    cancellationToken);
            }
            catch (Exception compensacaoException)
            {
                logger.LogCritical(
                    compensacaoException,
                    "Falha crítica ao compensar estoque da Nota Fiscal {NotaFiscalId}.",
                    notaFiscal.Id);
                falhaCompensacao = new FalhaCompensacaoEstoqueException(compensacaoException);
            }
        }

        try
        {
            notaFiscal.LiberarProcessamento();
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception liberacaoException)
        {
            logger.LogError(liberacaoException, "Não foi possível liberar o processamento da nota {Numero}.", notaFiscal.Numero);
        }

        logger.LogWarning(exception, "Processamento da nota {Numero} não concluído; a nota permanece Aberta.", notaFiscal.Numero);
        return falhaCompensacao;
    }

    private static string CriarChaveIdempotencia(string operacao, Guid notaFiscalId)
    {
        return $"{operacao}-nota-{notaFiscalId:N}";
    }

    private static NotaFiscalResponse MapearDetalhe(NotaFiscal notaFiscal)
    {
        var itens = notaFiscal.Itens
            .OrderBy(item => item.CodigoProduto)
            .Select(item => new ItemNotaFiscalResponse(
                item.ProdutoId,
                item.CodigoProduto,
                item.DescricaoProduto,
                item.Quantidade))
            .ToList();

        return new NotaFiscalResponse(
            notaFiscal.Id,
            notaFiscal.Numero,
            notaFiscal.Status.ToString(),
            notaFiscal.CriadaEm,
            notaFiscal.FechadaEm,
            itens,
            notaFiscal.NomeArquivo,
            notaFiscal.GeradaEm);
    }
}
