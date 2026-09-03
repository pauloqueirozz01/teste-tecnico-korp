using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class FalhaProcessamentoNotaFiscalException(Exception? innerException = null)
    : BillingDomainException(
        "FALHA_PROCESSAMENTO_NOTA",
        "Não foi possível concluir o processamento da nota fiscal.",
        StatusCodes.Status500InternalServerError)
{
    public Exception? Causa { get; } = innerException;
}
