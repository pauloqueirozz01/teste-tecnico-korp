using BillingService.Tests.TestHelpers;
using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Services;
using KorpTeste.BillingService.Domain.Enums;
using KorpTeste.BillingService.Domain.Exceptions;
using KorpTeste.BillingService.Infrastructure.Persistence;

namespace BillingService.Tests;

public sealed class ProcessamentoNotaFiscalServiceTests
{
    [Fact]
    public async Task ProcessarAsync_DeveConsumirEstoqueGerarArquivoEFecharNota()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var inventory = new InventoryClientFake();
        var gerador = new GeradorArquivoNotaFiscalFake();
        var service = CriarService(context, inventory, gerador);

        var resposta = await service.ProcessarAsync(nota.Id, CancellationToken.None);

        Assert.Equal("Fechada", resposta.NotaFiscal.Status);
        Assert.NotNull(resposta.NotaFiscal.FechadaEm);
        Assert.Equal("NF-000001.txt", resposta.NomeArquivo);
        Assert.Single(inventory.Consumos);
        Assert.Equal(StatusNotaFiscal.Fechada, context.NotasFiscais.Single().Status);
    }

    [Fact]
    public async Task ProcessarAsync_DeveManterNotaAbertaQuandoEstoqueFalhar()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var inventory = new InventoryClientFake { SaldoInsuficiente = true };
        var service = CriarService(context, inventory, new GeradorArquivoNotaFiscalFake());

        await Assert.ThrowsAsync<InventoryServiceRespostaException>(() => service.ProcessarAsync(nota.Id, CancellationToken.None));

        var persistida = context.NotasFiscais.Single();
        Assert.Equal(StatusNotaFiscal.Aberta, persistida.Status);
        Assert.Null(persistida.FechadaEm);
        Assert.Empty(inventory.Reposicoes);
    }

    [Fact]
    public async Task ProcessarAsync_DeveManterNotaAbertaQuandoInventoryEstiverIndisponivel()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var inventory = new InventoryClientFake { Indisponivel = true };
        var service = CriarService(context, inventory, new GeradorArquivoNotaFiscalFake());

        await Assert.ThrowsAsync<InventoryServiceIndisponivelException>(() => service.ProcessarAsync(nota.Id, CancellationToken.None));

        Assert.Equal(StatusNotaFiscal.Aberta, context.NotasFiscais.Single().Status);
    }

    [Fact]
    public async Task ProcessarAsync_DeveReporEstoqueQuandoFinalizacaoDoArquivoFalhar()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var inventory = new InventoryClientFake();
        var gerador = new GeradorArquivoNotaFiscalFake { FalharAoFinalizar = true };
        var service = CriarService(context, inventory, gerador);

        await Assert.ThrowsAsync<FalhaGeracaoArquivoException>(() => service.ProcessarAsync(nota.Id, CancellationToken.None));

        Assert.Single(inventory.Consumos);
        Assert.Single(inventory.Reposicoes);
        Assert.Equal(StatusNotaFiscal.Aberta, context.NotasFiscais.Single().Status);
    }

    [Fact]
    public async Task ProcessarAsync_DeveRetornarFalhaCompensacaoQuandoReposicaoFalhar()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var inventory = new InventoryClientFake { FalharAoRepor = true };
        var gerador = new GeradorArquivoNotaFiscalFake { FalharAoFinalizar = true };
        var service = CriarService(context, inventory, gerador);

        await Assert.ThrowsAsync<FalhaCompensacaoEstoqueException>(() => service.ProcessarAsync(nota.Id, CancellationToken.None));

        Assert.Single(inventory.Consumos);
        Assert.Empty(inventory.Reposicoes);
        Assert.Equal(StatusNotaFiscal.Aberta, context.NotasFiscais.Single().Status);
    }

    [Fact]
    public async Task ProcessarAsync_DeveRejeitarNotaFechadaSemConsumirNovamente()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var inventory = new InventoryClientFake();
        var gerador = new GeradorArquivoNotaFiscalFake();
        var service = CriarService(context, inventory, gerador);
        await service.ProcessarAsync(nota.Id, CancellationToken.None);

        await Assert.ThrowsAsync<NotaFiscalJaFechadaException>(() => service.ProcessarAsync(nota.Id, CancellationToken.None));

        Assert.Single(inventory.Consumos);
    }

    [Fact]
    public async Task ObterArquivoAsync_DeveRejeitarNotaAberta()
    {
        await using var context = BillingTestContextFactory.CriarContexto();
        var nota = await CriarNotaAsync(context);
        var service = CriarService(context, new InventoryClientFake(), new GeradorArquivoNotaFiscalFake());

        await Assert.ThrowsAsync<NotaFiscalNaoProcessadaException>(() => service.ObterArquivoAsync(nota.Id, CancellationToken.None));
    }

    private static ProcessamentoNotaFiscalService CriarService(
        BillingDbContext context,
        InventoryClientFake inventory,
        GeradorArquivoNotaFiscalFake gerador)
    {
        return new ProcessamentoNotaFiscalService(
            context,
            inventory,
            gerador,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ProcessamentoNotaFiscalService>.Instance);
    }

    private static async Task<KorpTeste.BillingService.Domain.Entities.NotaFiscal> CriarNotaAsync(BillingDbContext context)
    {
        var notaService = new NotaFiscalService(context, new SequencialNotaFiscalFake());
        var resposta = await notaService.CriarAsync(
            new CriarNotaFiscalRequest(
            [new CriarItemNotaFiscalRequest(
                Guid.NewGuid(),
                "PROD-001",
                "Teclado",
                2)]),
            CancellationToken.None);

        return context.NotasFiscais.Single(nota => nota.Id == resposta.Id);
    }
}
