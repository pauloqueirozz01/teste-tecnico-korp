namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ConsumirEstoqueResponse(
    string Mensagem,
    IReadOnlyCollection<ItemEstoqueResponse> Itens);

