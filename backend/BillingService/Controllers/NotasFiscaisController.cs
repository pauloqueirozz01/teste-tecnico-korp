using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KorpTeste.BillingService.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public sealed class NotasFiscaisController(
    INotaFiscalService notaFiscalService,
    IProcessamentoNotaFiscalService processamentoService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarAsync(
        CriarNotaFiscalRequest request,
        CancellationToken cancellationToken)
    {
        var notaFiscal = await notaFiscalService.CriarAsync(request, cancellationToken);

        return CreatedAtRoute(
            "ObterNotaFiscalPorId",
            new { id = notaFiscal.Id },
            notaFiscal);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotaFiscalResumoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<NotaFiscalResumoResponse>>> ListarAsync(
        CancellationToken cancellationToken)
    {
        var notasFiscais = await notaFiscalService.ListarAsync(cancellationToken);

        return Ok(notasFiscais);
    }

    [HttpGet("{id:guid}", Name = "ObterNotaFiscalPorId")]
    [ProducesResponseType(typeof(NotaFiscalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaFiscalResponse>> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notaFiscal = await notaFiscalService.ObterPorIdAsync(id, cancellationToken);

        return Ok(notaFiscal);
    }

    [HttpPost("{id:guid}/processar")]
    [ProducesResponseType(typeof(ResultadoProcessamentoNotaFiscalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ResultadoProcessamentoNotaFiscalResponse>> Processar(
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await processamentoService.ProcessarAsync(id, cancellationToken);
        return Ok(resultado);
    }

    [HttpGet("{id:guid}/arquivo")]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErroHttpResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ObterArquivo(
        Guid id,
        CancellationToken cancellationToken)
    {
        var arquivo = await processamentoService.ObterArquivoAsync(id, cancellationToken);
        return PhysicalFile(arquivo.CaminhoFisico, "text/plain", arquivo.NomeArquivo);
    }
}
