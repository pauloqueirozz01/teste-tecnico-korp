using System.ComponentModel.DataAnnotations;

namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ReporEstoqueRequest(
    [Required(ErrorMessage = "Informe ao menos um item para reposição de estoque.")]
    [MinLength(1, ErrorMessage = "Informe ao menos um item para reposição de estoque.")]
    IReadOnlyCollection<ItemEstoqueRequest> Itens);
