namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class InventoryServiceRespostaException(
    string codigo,
    string mensagem,
    int statusCode)
    : BillingDomainException(codigo, mensagem, statusCode);
