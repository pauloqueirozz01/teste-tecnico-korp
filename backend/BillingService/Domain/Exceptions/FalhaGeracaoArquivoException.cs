using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class FalhaGeracaoArquivoException(Exception? innerException = null)
    : BillingDomainException(
        "FALHA_GERACAO_ARQUIVO",
        "Não foi possível gerar o arquivo da nota fiscal.",
        StatusCodes.Status500InternalServerError)
{
    public Exception? Causa { get; } = innerException;
}
