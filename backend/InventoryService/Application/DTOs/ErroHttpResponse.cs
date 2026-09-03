using System.Text.Json.Serialization;

namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ErroHttpResponse(
    [property: JsonPropertyName("codigo")] string Codigo,
    [property: JsonPropertyName("mensagem")] string Mensagem,
    [property: JsonPropertyName("status")] int Status);

