using Microsoft.AspNetCore.Http;

namespace KorpTeste.InventoryService.Domain.Exceptions;

public sealed class SaldoInsuficienteException(string mensagem = "O produto não possui saldo suficiente para esta operação.")
    : InventoryDomainException("SALDO_INSUFICIENTE", mensagem, StatusCodes.Status409Conflict);
