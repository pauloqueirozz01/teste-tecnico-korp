using System.ComponentModel.DataAnnotations;

namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ValidarEstoqueRequest(
    [Required(ErrorMessage = "Informe ao menos um item para validação de estoque.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item para validação de estoque.")]
    IReadOnlyCollection<ItemEstoqueRequest> Itens);
