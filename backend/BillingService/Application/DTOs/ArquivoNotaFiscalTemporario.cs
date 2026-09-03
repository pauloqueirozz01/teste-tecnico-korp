namespace KorpTeste.BillingService.Application.DTOs;

public sealed record ArquivoNotaFiscalTemporario(
    string NomeArquivo,
    string CaminhoRelativo,
    string CaminhoTemporario,
    string CaminhoDefinitivo);
