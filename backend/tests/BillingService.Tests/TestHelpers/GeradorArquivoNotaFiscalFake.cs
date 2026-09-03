using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Domain.Entities;
using KorpTeste.BillingService.Domain.Exceptions;

namespace BillingService.Tests.TestHelpers;

internal sealed class GeradorArquivoNotaFiscalFake : IGeradorArquivoNotaFiscal
{
    public bool FalharAoGerar { get; set; }
    public bool FalharAoFinalizar { get; set; }
    public List<ArquivoNotaFiscalTemporario> Arquivos { get; } = [];

    public Task<ArquivoNotaFiscalTemporario> GerarTemporarioAsync(NotaFiscal notaFiscal, CancellationToken cancellationToken)
    {
        if (FalharAoGerar)
        {
            throw new FalhaGeracaoArquivoException();
        }

        var arquivo = new ArquivoNotaFiscalTemporario(
            $"NF-{notaFiscal.Numero:D6}.txt",
            $"storage/notas-fiscais/NF-{notaFiscal.Numero:D6}.txt",
            $"/tmp/NF-{notaFiscal.Numero:D6}.tmp",
            $"/tmp/NF-{notaFiscal.Numero:D6}.txt");
        Arquivos.Add(arquivo);
        return Task.FromResult(arquivo);
    }

    public Task FinalizarAsync(ArquivoNotaFiscalTemporario arquivo, CancellationToken cancellationToken)
    {
        if (FalharAoFinalizar)
        {
            throw new FalhaGeracaoArquivoException();
        }

        return Task.CompletedTask;
    }

    public Task ExcluirAsync(ArquivoNotaFiscalTemporario arquivo, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task ExcluirDefinitivoAsync(ArquivoNotaFiscalTemporario arquivo, CancellationToken cancellationToken) => Task.CompletedTask;

    public ArquivoNotaFiscalDownload ObterDownload(NotaFiscal notaFiscal) =>
        new("/tmp/arquivo-nota.txt", notaFiscal.NomeArquivo!);
}
