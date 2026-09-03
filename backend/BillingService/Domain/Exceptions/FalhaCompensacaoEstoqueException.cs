using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class FalhaCompensacaoEstoqueException(Exception? innerException = null)
    : BillingDomainException(
        "FALHA_COMPENSACAO_ESTOQUE",
        "Não foi possível compensar o estoque após a falha no processamento da nota fiscal. A nota permanece aberta para análise.",
        StatusCodes.Status500InternalServerError)
{
    public Exception? Causa { get; } = innerException;
}
