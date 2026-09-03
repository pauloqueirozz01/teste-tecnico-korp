using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Domain.Entities;

namespace KorpTeste.BillingService.Application.Interfaces;

public interface IGeradorArquivoNotaFiscal
{
    Task<ArquivoNotaFiscalTemporario> GerarTemporarioAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken);

    Task FinalizarAsync(
        ArquivoNotaFiscalTemporario arquivo,
        CancellationToken cancellationToken);

    Task ExcluirAsync(
        ArquivoNotaFiscalTemporario arquivo,
        CancellationToken cancellationToken);

    Task ExcluirDefinitivoAsync(
        ArquivoNotaFiscalTemporario arquivo,
        CancellationToken cancellationToken);

    ArquivoNotaFiscalDownload ObterDownload(NotaFiscal notaFiscal);
}
