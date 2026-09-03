namespace KorpTeste.BillingService.Application.Interfaces;

public interface INumeradorNotaFiscal
{
    Task<long> ProximoNumeroAsync(CancellationToken cancellationToken);
}
