namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ValidarEstoqueResponse(
    bool Valido,
    string Mensagem,
    IReadOnlyCollection<ItemEstoqueResponse> Itens);

