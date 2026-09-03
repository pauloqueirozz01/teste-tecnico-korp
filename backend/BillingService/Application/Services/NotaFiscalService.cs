using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Domain.Entities;
using KorpTeste.BillingService.Domain.Exceptions;
using KorpTeste.BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KorpTeste.BillingService.Application.Services;

public sealed class NotaFiscalService(
    BillingDbContext context,
    INumeradorNotaFiscal numeradorNotaFiscal)
    : INotaFiscalService
{
    public async Task<NotaFiscalResponse> CriarAsync(
        CriarNotaFiscalRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new RequisicaoInvalidaException("A requisição de criação da nota fiscal é obrigatória.");
        }

        var itens = CriarItensAgrupados(request);
        var numero = await numeradorNotaFiscal.ProximoNumeroAsync(cancellationToken);
        var notaFiscal = new NotaFiscal(numero, itens);

        context.NotasFiscais.Add(notaFiscal);
        await context.SaveChangesAsync(cancellationToken);

        return MapearDetalhe(notaFiscal);
    }

    public async Task<IReadOnlyCollection<NotaFiscalResumoResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        return await context.NotasFiscais
            .AsNoTracking()
            .OrderByDescending(notaFiscal => notaFiscal.Numero)
            .Select(notaFiscal => new NotaFiscalResumoResponse(
                notaFiscal.Id,
                notaFiscal.Numero,
                notaFiscal.Status.ToString(),
                notaFiscal.CriadaEm,
                notaFiscal.FechadaEm,
                notaFiscal.Itens.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<NotaFiscalResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var notaFiscal = await context.NotasFiscais
            .AsNoTracking()
            .Include(notaFiscal => notaFiscal.Itens)
            .FirstOrDefaultAsync(notaFiscal => notaFiscal.Id == id, cancellationToken);

        if (notaFiscal is null)
        {
            throw new NotaFiscalNaoEncontradaException(id);
        }

        return MapearDetalhe(notaFiscal);
    }

    private static IReadOnlyCollection<ItemNotaFiscal> CriarItensAgrupados(CriarNotaFiscalRequest request)
    {
        if (request.Itens is null || request.Itens.Count == 0)
        {
            throw new NotaFiscalSemItensException();
        }

        var itensNormalizados = request.Itens
            .Select(item => new ItemNotaFiscalEntrada(
                item.ProdutoId,
                item.CodigoProduto?.Trim().ToUpperInvariant() ?? string.Empty,
                item.DescricaoProduto?.Trim() ?? string.Empty,
                item.Quantidade))
            .ToList();

        var gruposInconsistentes = itensNormalizados
            .GroupBy(item => item.ProdutoId)
            .Where(grupo =>
                grupo.Select(item => item.CodigoProduto).Distinct(StringComparer.Ordinal).Count() > 1 ||
                grupo.Select(item => item.DescricaoProduto).Distinct(StringComparer.Ordinal).Count() > 1)
            .ToList();

        if (gruposInconsistentes.Count > 0)
        {
            throw new ProdutoItemInvalidoException(
                "Itens repetidos do mesmo produto devem possuir o mesmo código e a mesma descrição.");
        }

        return itensNormalizados
            .GroupBy(item => item.ProdutoId)
            .Select(grupo =>
            {
                var primeiroItem = grupo.First();
                return new ItemNotaFiscal(
                    primeiroItem.ProdutoId,
                    primeiroItem.CodigoProduto,
                    primeiroItem.DescricaoProduto,
                    grupo.Sum(item => item.Quantidade));
            })
            .OrderBy(item => item.CodigoProduto)
            .ToList();
    }

    private static NotaFiscalResponse MapearDetalhe(NotaFiscal notaFiscal)
    {
        var itens = notaFiscal.Itens
            .OrderBy(item => item.CodigoProduto)
            .Select(item => new ItemNotaFiscalResponse(
                item.ProdutoId,
                item.CodigoProduto,
                item.DescricaoProduto,
                item.Quantidade))
            .ToList();

        return new NotaFiscalResponse(
            notaFiscal.Id,
            notaFiscal.Numero,
            notaFiscal.Status.ToString(),
            notaFiscal.CriadaEm,
            notaFiscal.FechadaEm,
            itens,
            notaFiscal.NomeArquivo,
            notaFiscal.GeradaEm);
    }

    private sealed record ItemNotaFiscalEntrada(
        Guid ProdutoId,
        string CodigoProduto,
        string DescricaoProduto,
        int Quantidade);
}
