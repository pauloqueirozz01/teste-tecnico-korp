using System.ComponentModel.DataAnnotations;

namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ConsumirEstoqueRequest(
    [Required(ErrorMessage = "Informe ao menos um item para consumo de estoque.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item para consumo de estoque.")]
    IReadOnlyCollection<ItemEstoqueRequest> Itens);
