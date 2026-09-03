using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class RequisicaoInvalidaException(string mensagem)
    : BillingDomainException(
        "REQUISICAO_INVALIDA",
        mensagem,
        StatusCodes.Status400BadRequest);
