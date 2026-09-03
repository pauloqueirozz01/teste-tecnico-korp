using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class NotaFiscalJaFechadaException()
    : BillingDomainException(
        "NOTA_FISCAL_JA_FECHADA",
        "A nota fiscal já foi processada e não pode ser processada novamente.",
        StatusCodes.Status409Conflict);
