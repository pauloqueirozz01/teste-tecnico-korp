using InventoryService.Tests.TestHelpers;
using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Application.Services;
using KorpTeste.InventoryService.Domain.Entities;
using KorpTeste.InventoryService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Tests;

public class ProdutoServiceTests
{
    [Fact]
    public async Task CriarAsync_DeveCadastrarProdutoValido()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var service = new ProdutoService(context);

        var produto = await service.CriarAsync(
            new CriarProdutoRequest("prod-001", "Teclado Mecânico", 10),
            CancellationToken.None);

        Assert.Equal("PROD-001", produto.Codigo);
        Assert.Equal("Teclado Mecânico", produto.Descricao);
        Assert.Equal(10, produto.Saldo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CriarAsync_DeveRejeitarCodigoObrigatorio(string codigo)
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var service = new ProdutoService(context);

        await Assert.ThrowsAsync<RequisicaoInvalidaException>(() =>
            service.CriarAsync(new CriarProdutoRequest(codigo, "Produto", 1), CancellationToken.None));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CriarAsync_DeveRejeitarDescricaoObrigatoria(string descricao)
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var service = new ProdutoService(context);

        await Assert.ThrowsAsync<RequisicaoInvalidaException>(() =>
            service.CriarAsync(new CriarProdutoRequest("PROD-001", descricao, 1), CancellationToken.None));
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarSaldoNegativo()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var service = new ProdutoService(context);

        await Assert.ThrowsAsync<RequisicaoInvalidaException>(() =>
            service.CriarAsync(new CriarProdutoRequest("PROD-001", "Produto", -1), CancellationToken.None));
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarCodigoDuplicado()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var service = new ProdutoService(context);

        await service.CriarAsync(new CriarProdutoRequest("PROD-001", "Produto A", 1), CancellationToken.None);

        await Assert.ThrowsAsync<CodigoProdutoDuplicadoException>(() =>
            service.CriarAsync(new CriarProdutoRequest(" prod-001 ", "Produto B", 1), CancellationToken.None));
    }

    [Fact]
    public async Task ListarAsync_DeveOrdenarProdutosPorCodigo()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        context.Produtos.Add(new Produto("PROD-002", "Produto B", 1));
        context.Produtos.Add(new Produto("PROD-001", "Produto A", 1));
        await context.SaveChangesAsync();

        var service = new ProdutoService(context);

        var produtos = await service.ListarAsync(CancellationToken.None);

        Assert.Equal(["PROD-001", "PROD-002"], produtos.Select(produto => produto.Codigo));
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveConsultarProdutoExistente()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var produto = new Produto("PROD-001", "Produto", 3);
        context.Produtos.Add(produto);
        await context.SaveChangesAsync();

        var service = new ProdutoService(context);

        var resultado = await service.ObterPorIdAsync(produto.Id, CancellationToken.None);

        Assert.Equal(produto.Id, resultado.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarErroQuandoProdutoNaoExistir()
    {
        await using var context = InventoryTestContextFactory.CriarContexto();
        var service = new ProdutoService(context);

        await Assert.ThrowsAsync<ProdutoNaoEncontradoException>(() =>
            service.ObterPorIdAsync(Guid.NewGuid(), CancellationToken.None));
    }
}

