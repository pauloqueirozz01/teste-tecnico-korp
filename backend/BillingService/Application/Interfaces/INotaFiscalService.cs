using KorpTeste.BillingService.Application.DTOs;

namespace KorpTeste.BillingService.Application.Interfaces;

public interface INotaFiscalService
{
    Task<NotaFiscalResponse> CriarAsync(CriarNotaFiscalRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<NotaFiscalResumoResponse>> ListarAsync(CancellationToken cancellationToken);

    Task<NotaFiscalResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
}
