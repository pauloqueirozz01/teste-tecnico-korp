namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ProdutoResponse(
    Guid Id,
    string Codigo,
    string Descricao,
    int Saldo,
    DateTimeOffset CriadoEm,
    DateTimeOffset AtualizadoEm);

