namespace KorpTeste.BillingService.Application.DTOs;

public sealed record NotaFiscalResumoResponse(
    Guid Id,
    long Numero,
    string Status,
    DateTimeOffset CriadaEm,
    DateTimeOffset? FechadaEm,
    int QuantidadeItens);
