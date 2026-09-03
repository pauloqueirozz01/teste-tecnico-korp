namespace KorpTeste.InventoryService.Domain.Entities;

public sealed class OperacaoEstoqueIdempotente
{
    private OperacaoEstoqueIdempotente()
    {
    }

    public OperacaoEstoqueIdempotente(string chave, string tipo, string respostaJson)
    {
        Id = Guid.NewGuid();
        Chave = chave;
        Tipo = tipo;
        RespostaJson = respostaJson;
        CriadaEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Chave { get; private set; } = string.Empty;
    public string Tipo { get; private set; } = string.Empty;
    public string RespostaJson { get; private set; } = string.Empty;
    public DateTimeOffset CriadaEm { get; private set; }
}
