using System.ComponentModel.DataAnnotations;

namespace KorpTeste.BillingService.Application.DTOs;

public sealed record CriarNotaFiscalRequest(
    [Required(ErrorMessage = "Os itens da nota fiscal são obrigatórios.")]
    [MinLength(1, ErrorMessage = "A nota fiscal deve possuir pelo menos um item.")]
    IReadOnlyCollection<CriarItemNotaFiscalRequest> Itens);
