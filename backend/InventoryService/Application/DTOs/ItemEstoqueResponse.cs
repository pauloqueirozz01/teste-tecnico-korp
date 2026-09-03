namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ItemEstoqueResponse(
    Guid ProdutoId,
    string Codigo,
    int QuantidadeSolicitada,
    int SaldoAtual);

