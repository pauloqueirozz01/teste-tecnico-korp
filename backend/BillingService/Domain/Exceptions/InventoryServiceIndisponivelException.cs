using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class InventoryServiceIndisponivelException(Exception? innerException = null)
    : BillingDomainException(
        "INVENTORY_SERVICE_INDISPONIVEL",
        "O serviço de estoque está temporariamente indisponível. Tente novamente em alguns instantes.",
        StatusCodes.Status503ServiceUnavailable)
{
    public Exception? Causa { get; } = innerException;
}
