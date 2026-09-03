using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class QuantidadeItemInvalidaException()
    : BillingDomainException(
        "QUANTIDADE_ITEM_INVALIDA",
        "A quantidade do item deve ser maior que zero.",
        StatusCodes.Status400BadRequest);
