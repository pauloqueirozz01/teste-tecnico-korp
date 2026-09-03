using System.ComponentModel.DataAnnotations;
using KorpTeste.InventoryService.Infrastructure.Persistence.Configurations;

namespace KorpTeste.InventoryService.Application.DTOs;

public sealed record CriarProdutoRequest(
    [Required(ErrorMessage = "O código do produto é obrigatório.")]
    [MaxLength(ProdutoConfiguration.CodigoMaxLength, ErrorMessage = "O código do produto deve ter no máximo 50 caracteres.")]
    string Codigo,

    [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
    [MaxLength(ProdutoConfiguration.DescricaoMaxLength, ErrorMessage = "A descrição do produto deve ter no máximo 200 caracteres.")]
    string Descricao,

    [Range(0, int.MaxValue, ErrorMessage = "O saldo do produto não pode ser negativo.")]
    int Saldo);
