namespace KorpTeste.BillingService.Application.DTOs;

public sealed record ItemConsumoEstoqueRequest(
    Guid ProdutoId,
    int Quantidade);
