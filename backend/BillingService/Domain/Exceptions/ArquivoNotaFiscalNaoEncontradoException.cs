using Microsoft.AspNetCore.Http;

namespace KorpTeste.BillingService.Domain.Exceptions;

public sealed class ArquivoNotaFiscalNaoEncontradoException()
    : BillingDomainException(
        "ARQUIVO_NOTA_FISCAL_NAO_ENCONTRADO",
        "O arquivo da nota fiscal não foi encontrado no armazenamento.",
        StatusCodes.Status500InternalServerError);
