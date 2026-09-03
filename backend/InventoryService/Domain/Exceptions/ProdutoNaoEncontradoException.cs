using Microsoft.AspNetCore.Http;

namespace KorpTeste.InventoryService.Domain.Exceptions;

public sealed class ProdutoNaoEncontradoException(string mensagem = "Produto não encontrado.")
    : InventoryDomainException("PRODUTO_NAO_ENCONTRADO", mensagem, StatusCodes.Status404NotFound);
