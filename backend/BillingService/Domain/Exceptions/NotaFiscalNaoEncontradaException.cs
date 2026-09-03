using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class NotaFiscalNaoEncontradaException(Guid id)
    : BillingDomainException(
        "NOTA_FISCAL_NAO_ENCONTRADA",
        $"A nota fiscal '{id}' não foi encontrada.",
        StatusCodes.Status404NotFound);
