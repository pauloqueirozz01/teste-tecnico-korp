namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ReporEstoqueResponse(
    string Mensagem,
    IReadOnlyCollection<ItemEstoqueResponse> Itens);
