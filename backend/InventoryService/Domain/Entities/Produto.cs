using KorpTeste.InventoryService.Domain.Exceptions;

namespace KorpTeste.InventoryService.Domain.Entities;

public sealed class Produto
{
    private Produto()
    {
    }

    public Produto(string codigo, string descricao, int saldo)
    {
        Id = Guid.NewGuid();
        Codigo = codigo;
        Descricao = descricao;
        Saldo = saldo;
        CriadoEm = DateTimeOffset.UtcNow;
        AtualizadoEm = CriadoEm;
    }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public int Saldo { get; private set; }
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset AtualizadoEm { get; private set; }

    public void ConsumirEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new QuantidadeInvalidaException();
        }

        if (Saldo < quantidade)
        {
            throw new SaldoInsuficienteException();
        }

        Saldo -= quantidade;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }

    public void ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new QuantidadeInvalidaException();
        }

        Saldo += quantidade;
        AtualizadoEm = DateTimeOffset.UtcNow;
    }
}
