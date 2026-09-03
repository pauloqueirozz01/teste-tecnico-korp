using Microsoft.AspNetCore.Http;

namespace KorpTeste.InventoryService.Domain.Exceptions;

public sealed class QuantidadeInvalidaException(string mensagem = "A quantidade deve ser maior que zero.")
    : InventoryDomainException("QUANTIDADE_INVALIDA", mensagem, StatusCodes.Status400BadRequest);
