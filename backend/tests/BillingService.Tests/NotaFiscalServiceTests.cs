using BillingService.Tests.TestHelpers;
using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Services;
using KorpTeste.BillingService.Domain.Enums;
using KorpTeste.BillingService.Domain.Exceptions;
using KorpTeste.BillingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Tests;

public class NotaFiscalServiceTests
{
    [Fact]
    public async Task CriarAsync_DeveCriarNotaAbertaComNumeroGeradoPeloBackend()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        var resposta = await service.CriarAsync(CriarRequestPadrao(), CancellationToken.None);

        Assert.Equal(1, resposta.Numero);
        Assert.Equal(StatusNotaFiscal.Aberta.ToString(), resposta.Status);
        Assert.Null(resposta.FechadaEm);
        Assert.Single(resposta.Itens);
    }

    [Fact]
    public async Task CriarAsync_DeveGerarNumerosSequenciais()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        var primeira = await service.CriarAsync(CriarRequestPadrao("PROD-001"), CancellationToken.None);
        var segunda = await service.CriarAsync(CriarRequestPadrao("PROD-002"), CancellationToken.None);

        Assert.Equal(1, primeira.Numero);
        Assert.Equal(2, segunda.Numero);
    }

    [Fact]
    public async Task CriarAsync_DevePersistirMultiplosProdutos()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        var resposta = await service.CriarAsync(new CriarNotaFiscalRequest(
        [
            CriarItem("PROD-001", quantidade: 2),
            CriarItem("PROD-002", quantidade: 3)
        ]), CancellationToken.None);

        Assert.Equal(2, resposta.Itens.Count);
        Assert.Contains(resposta.Itens, item => item.CodigoProduto == "PROD-001" && item.Quantidade == 2);
        Assert.Contains(resposta.Itens, item => item.CodigoProduto == "PROD-002" && item.Quantidade == 3);
    }

    [Fact]
    public async Task CriarAsync_DeveAgruparProdutoRepetido()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var produtoId = Guid.NewGuid();
        var service = CriarService(context);

        var resposta = await service.CriarAsync(new CriarNotaFiscalRequest(
        [
            CriarItem("PROD-001", produtoId, 2),
            CriarItem("PROD-001", produtoId, 3)
        ]), CancellationToken.None);

        var item = Assert.Single(resposta.Itens);
        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarNotaSemItens()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        await Assert.ThrowsAsync<NotaFiscalSemItensException>(() =>
            service.CriarAsync(new CriarNotaFiscalRequest([]), CancellationToken.None));

        Assert.Empty(context.NotasFiscais);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CriarAsync_DeveRejeitarQuantidadeInvalida(int quantidade)
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        await Assert.ThrowsAsync<QuantidadeItemInvalidaException>(() =>
            service.CriarAsync(new CriarNotaFiscalRequest([CriarItem("PROD-001", quantidade: quantidade)]), CancellationToken.None));

        Assert.Empty(context.NotasFiscais);
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarProdutoIdInvalido()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        await Assert.ThrowsAsync<ProdutoItemInvalidoException>(() =>
            service.CriarAsync(new CriarNotaFiscalRequest([CriarItem("PROD-001", Guid.Empty, 1)]), CancellationToken.None));
    }

    [Fact]
    public async Task CriarAsync_DeveRejeitarSnapshotInconsistenteParaProdutoRepetido()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var produtoId = Guid.NewGuid();
        var service = CriarService(context);

        await Assert.ThrowsAsync<ProdutoItemInvalidoException>(() =>
            service.CriarAsync(new CriarNotaFiscalRequest(
            [
                CriarItem("PROD-001", produtoId, 1),
                new CriarItemNotaFiscalRequest(produtoId, "PROD-002", "Produto diferente", 1)
            ]), CancellationToken.None));
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNotaExistente()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);
        var criada = await service.CriarAsync(CriarRequestPadrao(), CancellationToken.None);

        var resposta = await service.ObterPorIdAsync(criada.Id, CancellationToken.None);

        Assert.Equal(criada.Numero, resposta.Numero);
        Assert.Equal(criada.Status, resposta.Status);
        Assert.Equal(criada.Itens.Count, resposta.Itens.Count);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRejeitarNotaInexistente()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);

        await Assert.ThrowsAsync<NotaFiscalNaoEncontradaException>(() =>
            service.ObterPorIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task ListarAsync_DeveOrdenarPorNumeroDecrescente()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var service = CriarService(context);
        await service.CriarAsync(CriarRequestPadrao("PROD-001"), CancellationToken.None);
        await service.CriarAsync(CriarRequestPadrao("PROD-002"), CancellationToken.None);

        var resposta = await service.ListarAsync(CancellationToken.None);

        Assert.Equal([2, 1], resposta.Select(nota => nota.Numero));
    }

    private static NotaFiscalService CriarService(BillingDbContext context)
    {
        return new NotaFiscalService(context, new SequencialNotaFiscalFake());
    }

    private static CriarNotaFiscalRequest CriarRequestPadrao(string codigoProduto = "PROD-001")
    {
        return new CriarNotaFiscalRequest([CriarItem(codigoProduto)]);
    }

    private static CriarItemNotaFiscalRequest CriarItem(
        string codigoProduto,
        Guid? produtoId = null,
        int quantidade = 1)
    {
        return new CriarItemNotaFiscalRequest(
            produtoId ?? Guid.NewGuid(),
            codigoProduto,
            $"Produto {codigoProduto}",
            quantidade);
    }
}
