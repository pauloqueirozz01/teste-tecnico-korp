using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class ProdutoItemInvalidoException(string mensagem)
    : BillingDomainException(
        "PRODUTO_ITEM_INVALIDO",
        mensagem,
        StatusCodes.Status400BadRequest);
