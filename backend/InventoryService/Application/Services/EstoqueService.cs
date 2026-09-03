using System.Data;
using System.Text.Json;
using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Application.Interfaces;
using KorpTeste.InventoryService.Domain.Entities;
using KorpTeste.InventoryService.Domain.Exceptions;
using KorpTeste.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KorpTeste.InventoryService.Application.Services;

public sealed class EstoqueService(InventoryDbContext dbContext) : IEstoqueService
{
    public async Task<ValidarEstoqueResponse> ValidarAsync(
        ValidarEstoqueRequest request,
        CancellationToken cancellationToken)
    {
        var itensAgrupados = NormalizarItens(request.Itens);
        var produtos = await ObterProdutosAsync(itensAgrupados.Keys, cancellationToken);

        ValidarProdutosEncontrados(itensAgrupados.Keys, produtos);
        ValidarSaldos(itensAgrupados, produtos);

        var itens = MapearItens(itensAgrupados, produtos);

        return new ValidarEstoqueResponse(true, "Estoque disponível para a operação solicitada.", itens);
    }

    public async Task<ConsumirEstoqueResponse> ConsumirAsync(
        ConsumirEstoqueRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var itensAgrupados = NormalizarItens(request.Itens);

        if (!dbContext.Database.IsRelational())
        {
            return await ExecutarComIdempotenciaAsync(
                idempotencyKey,
                "consumo",
                () => ConsumirSemTransacaoRelacionalAsync(itensAgrupados, cancellationToken),
                cancellationToken);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var resposta = await ExecutarComIdempotenciaAsync(
                idempotencyKey,
                "consumo",
                () => ConsumirSemTransacaoRelacionalAsync(itensAgrupados, cancellationToken),
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return resposta;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ReporEstoqueResponse> ReporAsync(
        ReporEstoqueRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var itensAgrupados = NormalizarItens(request.Itens);

        if (!dbContext.Database.IsRelational())
        {
            return await ExecutarComIdempotenciaAsync(
                idempotencyKey,
                "compensacao",
                () => ReporSemTransacaoRelacionalAsync(itensAgrupados, cancellationToken),
                cancellationToken);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        try
        {
            var resposta = await ExecutarComIdempotenciaAsync(
                idempotencyKey,
                "compensacao",
                () => ReporSemTransacaoRelacionalAsync(itensAgrupados, cancellationToken),
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return resposta;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task<TResponse> ExecutarComIdempotenciaAsync<TResponse>(
        string? idempotencyKey,
        string tipo,
        Func<Task<TResponse>> executarAsync,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            return await executarAsync();
        }

        var operacaoExistente = await dbContext.OperacoesEstoqueIdempotentes
            .AsNoTracking()
            .SingleOrDefaultAsync(operacao => operacao.Chave == idempotencyKey, cancellationToken);

        if (operacaoExistente is not null)
        {
            return JsonSerializer.Deserialize<TResponse>(operacaoExistente.RespostaJson)
                ?? throw new RequisicaoInvalidaException("A resposta idempotente registrada é inválida.");
        }

        var resposta = await executarAsync();
        dbContext.OperacoesEstoqueIdempotentes.Add(new OperacaoEstoqueIdempotente(
            idempotencyKey,
            tipo,
            JsonSerializer.Serialize(resposta)));
        await dbContext.SaveChangesAsync(cancellationToken);

        return resposta;
    }

    private async Task<ConsumirEstoqueResponse> ConsumirSemTransacaoRelacionalAsync(
        IReadOnlyDictionary<Guid, int> itensAgrupados,
        CancellationToken cancellationToken)
    {
        var produtos = await ObterProdutosAsync(itensAgrupados.Keys, cancellationToken);

        ValidarProdutosEncontrados(itensAgrupados.Keys, produtos);
        ValidarSaldos(itensAgrupados, produtos);

        foreach (var item in itensAgrupados)
        {
            produtos[item.Key].ConsumirEstoque(item.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var itens = MapearItens(itensAgrupados, produtos);

        return new ConsumirEstoqueResponse("Estoque consumido com sucesso.", itens);
    }

    private async Task<ReporEstoqueResponse> ReporSemTransacaoRelacionalAsync(
        IReadOnlyDictionary<Guid, int> itensAgrupados,
        CancellationToken cancellationToken)
    {
        var produtos = await ObterProdutosAsync(itensAgrupados.Keys, cancellationToken);

        ValidarProdutosEncontrados(itensAgrupados.Keys, produtos);

        foreach (var item in itensAgrupados)
        {
            produtos[item.Key].ReporEstoque(item.Value);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var itens = MapearItens(itensAgrupados, produtos);

        return new ReporEstoqueResponse("Estoque reposto com sucesso.", itens);
    }

    private async Task<Dictionary<Guid, Produto>> ObterProdutosAsync(
        IEnumerable<Guid> produtoIds,
        CancellationToken cancellationToken)
    {
        var ids = produtoIds.ToArray();

        return await dbContext.Produtos
            .Where(produto => ids.Contains(produto.Id))
            .ToDictionaryAsync(produto => produto.Id, cancellationToken);
    }

    private static IReadOnlyDictionary<Guid, int> NormalizarItens(IReadOnlyCollection<ItemEstoqueRequest>? itens)
    {
        if (itens is null || itens.Count == 0)
        {
            throw new RequisicaoInvalidaException("Informe ao menos um item de estoque.");
        }

        if (itens.Any(item => item.ProdutoId == Guid.Empty))
        {
            throw new ProdutoNaoEncontradoException("Produto não encontrado para um ou mais itens informados.");
        }

        if (itens.Any(item => item.Quantidade <= 0))
        {
            throw new QuantidadeInvalidaException();
        }

        return itens
            .GroupBy(item => item.ProdutoId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));
    }

    private static void ValidarProdutosEncontrados(
        IEnumerable<Guid> produtoIds,
        IReadOnlyDictionary<Guid, Produto> produtos)
    {
        if (produtoIds.Any(produtoId => !produtos.ContainsKey(produtoId)))
        {
            throw new ProdutoNaoEncontradoException("Produto não encontrado para um ou mais itens informados.");
        }
    }

    private static void ValidarSaldos(
        IReadOnlyDictionary<Guid, int> itensAgrupados,
        IReadOnlyDictionary<Guid, Produto> produtos)
    {
        var saldoInsuficiente = itensAgrupados
            .Any(item => produtos[item.Key].Saldo < item.Value);

        if (saldoInsuficiente)
        {
            throw new SaldoInsuficienteException();
        }
    }

    private static IReadOnlyCollection<ItemEstoqueResponse> MapearItens(
        IReadOnlyDictionary<Guid, int> itensAgrupados,
        IReadOnlyDictionary<Guid, Produto> produtos)
    {
        return itensAgrupados
            .Select(item =>
            {
                var produto = produtos[item.Key];

                return new ItemEstoqueResponse(
                    produto.Id,
                    produto.Codigo,
                    item.Value,
                    produto.Saldo);
            })
            .OrderBy(item => item.Codigo)
            .ToList();
    }
}
