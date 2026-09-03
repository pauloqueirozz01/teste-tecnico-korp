using KorpTeste.BillingService.Application.Interfaces;

namespace BillingService.Tests.TestHelpers;

internal sealed class SequencialNotaFiscalFake : INumeradorNotaFiscal
{
    private long _numeroAtual;

    public Task<long> ProximoNumeroAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Interlocked.Increment(ref _numeroAtual));
    }
}
