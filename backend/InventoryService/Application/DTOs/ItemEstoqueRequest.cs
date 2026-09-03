using System.ComponentModel.DataAnnotations;

namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record ItemEstoqueRequest(
    [Required(ErrorMessage = "O produto é obrigatório.")]
    Guid ProdutoId,

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    int Quantidade);
