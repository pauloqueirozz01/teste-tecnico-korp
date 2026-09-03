using Microsoft.AspNetCore.Http;

namespace KorpTeste.InventoryService.Domain.Exceptions;

public sealed class CodigoProdutoDuplicadoException(string mensagem = "Já existe um produto cadastrado com este código.")
    : InventoryDomainException("CODIGO_PRODUTO_DUPLICADO", mensagem, StatusCodes.Status409Conflict);
