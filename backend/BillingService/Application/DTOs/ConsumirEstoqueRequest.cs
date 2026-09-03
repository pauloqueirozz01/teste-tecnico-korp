namespace KorpTeste.BillingService.Application.DTOs;

public sealed record ConsumirEstoqueRequest(
    IReadOnlyCollection<ItemConsumoEstoqueRequest> Itens);
