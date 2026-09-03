using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class NotaFiscalNaoProcessadaException()
    : BillingDomainException(
        "NOTA_FISCAL_NAO_PROCESSADA",
        "A nota fiscal ainda não foi processada.",
        StatusCodes.Status409Conflict);
