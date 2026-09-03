using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class NotaFiscalSemItensException()
    : BillingDomainException(
        "NOTA_FISCAL_SEM_ITENS",
        "A nota fiscal deve possuir pelo menos um item.",
        StatusCodes.Status400BadRequest);
