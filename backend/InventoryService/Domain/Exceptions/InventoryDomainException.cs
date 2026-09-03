namespace KorpTeste.InventoryService.Domain.Exceptions;

public abstract class InventoryDomainException(
    string codigo,
    string mensagem,
    int statusCode) : Exception(mensagem)
{
    public string Codigo { get; } = codigo;
    public int StatusCode { get; } = statusCode;
}

