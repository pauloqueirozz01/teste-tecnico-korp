namespace KorpTeste.BillingService.Application.DTOs;

public sealed record ItemNotaFiscalResponse(
    Guid ProdutoId,
    string CodigoProduto,
    string DescricaoProduto,
    int Quantidade);
