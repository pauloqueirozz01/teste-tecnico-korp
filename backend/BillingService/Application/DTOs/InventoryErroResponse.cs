namespace KorpTeste.BillingService.Application.DTOs;

public sealed record InventoryErroResponse(
    string Codigo,
    string Mensagem,
    int Status);
