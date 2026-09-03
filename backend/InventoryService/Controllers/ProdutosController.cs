using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KorpTeste.InventoryService.Controllers;

[ApiController]
[Route("api/produtos")]
[Produces("application/json")]
public sealed class ProdutosController(IProdutoService produtoService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarProdutoRequest request,
        CancellationToken cancellationToken)
    {
        var produto = await produtoService.CriarAsync(request, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var produtos = await produtoService.ListarAsync(cancellationToken);
        return Ok(produtos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var produto = await produtoService.ObterPorIdAsync(id, cancellationToken);
        return Ok(produto);
    }
}
