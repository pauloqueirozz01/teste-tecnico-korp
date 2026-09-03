using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KorpTeste.InventoryService.Controllers;

[ApiController]
[Route("api/estoque")]
[Produces("application/json")]
public sealed class EstoqueController(IEstoqueService estoqueService) : ControllerBase
{
    [HttpPost("validar")]
    [ProducesResponseType(typeof(ValidarEstoqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Validar(
        [FromBody] ValidarEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await estoqueService.ValidarAsync(request, cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("consumir")]
    [ProducesResponseType(typeof(ConsumirEstoqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Consumir(
        [FromBody] ConsumirEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await estoqueService.ConsumirAsync(
            request,
            ObterChaveIdempotencia(),
            cancellationToken);
        return Ok(resultado);
    }

    [HttpPost("repor")]
    [ProducesResponseType(typeof(ReporEstoqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Repor(
        [FromBody] ReporEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var resultado = await estoqueService.ReporAsync(
            request,
            ObterChaveIdempotencia(),
            cancellationToken);
        return Ok(resultado);
    }

    private string? ObterChaveIdempotencia()
    {
        return Request.Headers.TryGetValue("Idempotency-Key", out var valores)
            ? valores.FirstOrDefault()
            : null;
    }
}
