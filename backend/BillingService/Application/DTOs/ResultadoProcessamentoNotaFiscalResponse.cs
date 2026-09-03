namespace KorpTeste.BillingService.Application.DTOs;

public sealed record ResultadoProcessamentoNotaFiscalResponse(
    NotaFiscalResponse NotaFiscal,
    string NomeArquivo);
