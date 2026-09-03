using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Application.Interfaces;
using KorpTeste.InventoryService.Domain.Entities;
using KorpTeste.InventoryService.Domain.Exceptions;
using KorpTeste.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KorpTeste.InventoryService.Application.Services;

public sealed class ProdutoService(InventoryDbContext dbContext) : IProdutoService
{
    public async Task<ProdutoResponse> CriarAsync(CriarProdutoRequest request, CancellationToken cancellationToken)
    {
        var codigo = NormalizarCodigo(request.Codigo);
        var descricao = request.Descricao?.Trim() ?? string.Empty;

        ValidarProduto(codigo, descricao, request.Saldo);

        var codigoJaExiste = await dbContext.Produtos
            .AnyAsync(produto => produto.Codigo == codigo, cancellationToken);

        if (codigoJaExiste)
        {
            throw new CodigoProdutoDuplicadoException();
        }

        var produto = new Produto(codigo, descricao, request.Saldo);
        dbContext.Produtos.Add(produto);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new CodigoProdutoDuplicadoException();
        }

        return Mapear(produto);
    }

    public async Task<IReadOnlyCollection<ProdutoResponse>> ListarAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Codigo)
            .Select(produto => new ProdutoResponse(
                produto.Id,
                produto.Codigo,
                produto.Descricao,
                produto.Saldo,
                produto.CriadoEm,
                produto.AtualizadoEm))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProdutoResponse> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var produto = await dbContext.Produtos
            .AsNoTracking()
            .SingleOrDefaultAsync(produto => produto.Id == id, cancellationToken);

        if (produto is null)
        {
            throw new ProdutoNaoEncontradoException();
        }

        return Mapear(produto);
    }

    private static void ValidarProduto(string codigo, string descricao, int saldo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new RequisicaoInvalidaException("O código do produto é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new RequisicaoInvalidaException("A descrição do produto é obrigatória.");
        }

        if (saldo < 0)
        {
            throw new RequisicaoInvalidaException("O saldo do produto não pode ser negativo.");
        }
    }

    internal static ProdutoResponse Mapear(Produto produto)
    {
        return new ProdutoResponse(
            produto.Id,
            produto.Codigo,
            produto.Descricao,
            produto.Saldo,
            produto.CriadoEm,
            produto.AtualizadoEm);
    }

    internal static string NormalizarCodigo(string? codigo)
    {
        return (codigo ?? string.Empty).Trim().ToUpperInvariant();
    }
}

