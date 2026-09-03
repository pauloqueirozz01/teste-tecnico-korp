using InventoryService.Tests.TestHelpers;
using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Application.Services;
using KorpTeste.InventoryService.Domain.Entities;
using KorpTeste.InventoryService.Domain.Exceptions;
using KorpTeste.InventoryService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Tests;

public class EstoqueServiceTests
{
    [Fact]
    public async Task ValidarAsync_DeveConfirmarEstoqueSuficiente()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 10);
        var service = new EstoqueService(context);

        var resposta = await service.ValidarAsync(
            new ValidarEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]),
            CancellationToken.None);

        Assert.True(resposta.Valido);
        Assert.Equal(10, produto.Saldo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidarAsync_DeveRejeitarQuantidadeInvalida(int quantidade)
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 10);
        var service = new EstoqueService(context);

        await Assert.ThrowsAsync<QuantidadeInvalidaException>(() =>
            service.ValidarAsync(
                new ValidarEstoqueRequest([new ItemEstoqueRequest(produto.Id, quantidade)]),
                CancellationToken.None));
    }

    [Fact]
    public async Task ValidarAsync_DeveRejeitarEstoqueInsuficiente()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 1);
        var service = new EstoqueService(context);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            service.ValidarAsync(
                new ValidarEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]),
                CancellationToken.None));

        Assert.Equal(1, produto.Saldo);
    }

    [Fact]
    public async Task ConsumirAsync_DeveConsumirEstoque()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 10);
        var service = new EstoqueService(context);

        await service.ConsumirAsync(
            new ConsumirEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]),
            null,
            CancellationToken.None);

        Assert.Equal(8, produto.Saldo);
    }

    [Fact]
    public async Task ConsumirAsync_DeveConsumirMultiplosProdutos()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produtoA = await CriarProdutoAsync(context, "PROD-001", 10);
        var produtoB = await CriarProdutoAsync(context, "PROD-002", 5);
        var service = new EstoqueService(context);

        await service.ConsumirAsync(
            new ConsumirEstoqueRequest(
            [
                new ItemEstoqueRequest(produtoA.Id, 2),
                new ItemEstoqueRequest(produtoB.Id, 3)
            ]),
            null,
            CancellationToken.None);

        Assert.Equal(8, produtoA.Saldo);
        Assert.Equal(2, produtoB.Saldo);
    }

    [Fact]
    public async Task ConsumirAsync_DevePreservarSaldosQuandoOperacaoFalhar()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produtoA = await CriarProdutoAsync(context, "PROD-001", 10);
        var produtoB = await CriarProdutoAsync(context, "PROD-002", 0);
        var service = new EstoqueService(context);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            service.ConsumirAsync(
                new ConsumirEstoqueRequest(
                [
                    new ItemEstoqueRequest(produtoA.Id, 2),
                    new ItemEstoqueRequest(produtoB.Id, 1)
                ]),
                null,
                CancellationToken.None));

        Assert.Equal(10, produtoA.Saldo);
        Assert.Equal(0, produtoB.Saldo);
    }

    [Fact]
    public async Task ConsumirAsync_DeveAgruparProdutoRepetidoAntesDeValidarSaldo()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 5);
        var service = new EstoqueService(context);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            service.ConsumirAsync(
                new ConsumirEstoqueRequest(
                [
                    new ItemEstoqueRequest(produto.Id, 3),
                    new ItemEstoqueRequest(produto.Id, 3)
                ]),
                null,
                CancellationToken.None));

        Assert.Equal(5, produto.Saldo);
    }

    [Fact]
    public async Task ConsumirAsync_NaoDevePermitirSaldoNegativo()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 1);
        var service = new EstoqueService(context);

        await Assert.ThrowsAsync<SaldoInsuficienteException>(() =>
            service.ConsumirAsync(
                new ConsumirEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]),
                null,
                CancellationToken.None));

        Assert.Equal(1, produto.Saldo);
    }

    [Fact]
    public async Task ReporAsync_DeveReporEstoqueParaCompensacao()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 8);
        var service = new EstoqueService(context);

        await service.ReporAsync(
            new ReporEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]),
            null,
            CancellationToken.None);

        Assert.Equal(10, produto.Saldo);
    }

    [Fact]
    public async Task ConsumirAsync_DeveSerIdempotenteComMesmaChave()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 10);
        var service = new EstoqueService(context);
        var request = new ConsumirEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]);

        await service.ConsumirAsync(request, "consumo-nota-001", CancellationToken.None);
        await service.ConsumirAsync(request, "consumo-nota-001", CancellationToken.None);

        Assert.Equal(8, produto.Saldo);
    }

    [Fact]
    public async Task ReporAsync_DeveSerIdempotenteComMesmaChave()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = await CriarProdutoAsync(context, "PROD-001", 8);
        var service = new EstoqueService(context);
        var request = new ReporEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]);

        await service.ReporAsync(request, "compensacao-nota-001", CancellationToken.None);
        await service.ReporAsync(request, "compensacao-nota-001", CancellationToken.None);

        Assert.Equal(10, produto.Saldo);
    }

    private static async Task<Produto> CriarProdutoAsync(InventoryDbContext context, string codigo, int saldo)
    {
        var produto = new Produto(codigo, $"Produto {codigo}", saldo);
        context.Produtos.Add(produto);
        await context.SaveChangesAsync();
        return produto;
    }
}
