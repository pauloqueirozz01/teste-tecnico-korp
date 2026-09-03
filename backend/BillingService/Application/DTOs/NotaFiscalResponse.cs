namespace KorpTeste.BillingService.Application.DTOs;

public sealed record NotaFiscalResponse(
    Guid Id,
    long Numero,
    string Status,
    DateTimeOffset CriadaEm,
    DateTimeOffset? FechadaEm,
    IReadOnlyCollection<ItemNotaFiscalResponse> Itens,
    string? NomeArquivo = null,
    DateTimeOffset? GeradaEm = null);
