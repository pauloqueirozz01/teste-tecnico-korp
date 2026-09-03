namespace KorpTeste.BillingService.Domain.Exceptions;

public abstract class BillingDomainException(string codigo, string mensagem, int statusCode)
    : Exception(mensagem)
{
    public string Codigo { get; } = codigo;

    public int StatusCode { get; } = statusCode;
}
