using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class NotaFiscalEmProcessamentoException()
    : BillingDomainException(
        "NOTA_FISCAL_EM_PROCESSAMENTO",
        "A nota fiscal já possui um processamento em andamento.",
        StatusCodes.Status409Conflict);
