using KorpTeste.BillingService.Application.DTOs;

namespace KorpTeste.BillingService.Application.Interfaces;

public interface IProcessamentoNotaFiscalService
{
    Task<ResultadoProcessamentoNotaFiscalResponse> ProcessarAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<ArquivoNotaFiscalDownload> ObterArquivoAsync(
        Guid id,
        CancellationToken cancellationToken);
}
