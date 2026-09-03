using System.ComponentModel.DataAnnotations;

namespace KorpTeste.BillingService.Application.DTOs;

public sealed record CriarItemNotaFiscalRequest(
    [Required(ErrorMessage = "O produto do item da nota fiscal é obrigatório.")]
    Guid ProdutoId,
    [Required(ErrorMessage = "O código do produto do item da nota fiscal é obrigatório.")]
    [MaxLength(50, ErrorMessage = "O código do produto deve possuir no máximo 50 caracteres.")]
    string CodigoProduto,
    [Required(ErrorMessage = "A descrição do produto do item da nota fiscal é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição do produto deve possuir no máximo 200 caracteres.")]
    string DescricaoProduto,
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade do item deve ser maior que zero.")]
    int Quantidade);
