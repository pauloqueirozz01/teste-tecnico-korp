using Microsoft.AspNetCore.Http;

namespace KorpTeste.InventoryService.Domain.Exceptions;

public sealed class RequisicaoInvalidaException(string mensagem)
    : InventoryDomainException("REQUISICAO_INVALIDA", mensagem, StatusCodes.Status400BadRequest);
