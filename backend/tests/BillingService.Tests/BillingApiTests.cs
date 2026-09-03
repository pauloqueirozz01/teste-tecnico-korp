using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BillingService.Tests.TestHelpers;
using KorpTeste.BillingService.Application.DTOs;

namespace BillingService.Tests;

public class BillingApiTests(BillingApiFactory factory) : IClassFixture<BillingApiFactory>
{
    [Fact]
    public async Task PostNotasFiscais_DeveRetornarCreatedComNotaAberta()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var nota = await response.Content.ReadFromJsonAsync<NotaFiscalResponse>();
        Assert.NotNull(nota);
        Assert.True(nota.Numero > 0);
        Assert.Equal("Aberta", nota.Status);
        Assert.Single(nota.Itens);
    }

    [Fact]
    public async Task GetNotasFiscais_DeveListarNotasPersistidas()
    {
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao("PROD-001"));
        await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao("PROD-002"));

        var notas = await client.GetFromJsonAsync<IReadOnlyCollection<NotaFiscalResumoResponse>>("/api/notas-fiscais");

        Assert.NotNull(notas);
        Assert.True(notas.Count >= 2);
        Assert.Equal(notas.OrderByDescending(nota => nota.Numero).Select(nota => nota.Numero), notas.Select(nota => nota.Numero));
    }

    [Fact]
    public async Task GetNotasFiscaisPorId_DeveRetornarDetalhePersistido()
    {
        var client = factory.CreateClient();
        var post = await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao());
        var criada = await post.Content.ReadFromJsonAsync<NotaFiscalResponse>();

        var detalhe = await client.GetFromJsonAsync<NotaFiscalResponse>($"/api/notas-fiscais/{criada!.Id}");

        Assert.NotNull(detalhe);
        Assert.Equal(criada.Numero, detalhe.Numero);
        Assert.Equal("Aberta", detalhe.Status);
        Assert.Single(detalhe.Itens);
    }

    [Fact]
    public async Task GetNotasFiscaisPorId_DeveRetornarNotFoundParaNotaInexistente()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/notas-fiscais/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var erro = await response.Content.ReadFromJsonAsync<ErroHttpResponse>();
        Assert.NotNull(erro);
        Assert.Equal("NOTA_FISCAL_NAO_ENCONTRADA", erro.Codigo);
    }

    [Fact]
    public async Task PostNotasFiscais_DeveRetornarBadRequestParaNotaSemItens()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/notas-fiscais", new CriarNotaFiscalRequest([]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var erro = await response.Content.ReadFromJsonAsync<ErroHttpResponse>();
        Assert.NotNull(erro);
        Assert.Equal("REQUISICAO_INVALIDA", erro.Codigo);
    }

    [Fact]
    public async Task PostNotasFiscais_DeveAgruparProdutoRepetidoNoContratoHttp()
    {
        var client = factory.CreateClient();
        var produtoId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync("/api/notas-fiscais", new CriarNotaFiscalRequest(
        [
            CriarItem("PROD-001", produtoId, 2),
            CriarItem("PROD-001", produtoId, 3)
        ]));

        var nota = await response.Content.ReadFromJsonAsync<NotaFiscalResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = Assert.Single(nota!.Itens);
        Assert.Equal(5, item.Quantidade);
    }

    [Fact]
    public async Task Swagger_DeveExporEndpointDeNotasFiscais()
    {
        var client = factory.CreateClient();

        var document = await client.GetStringAsync("/swagger/v1/swagger.json");

        using var json = JsonDocument.Parse(document);
        var paths = json.RootElement.GetProperty("paths");

        Assert.True(paths.TryGetProperty("/api/notas-fiscais", out _));
        Assert.True(paths.TryGetProperty("/api/notas-fiscais/{id}", out _));
    }

    [Fact]
    public async Task ProcessarNotaFiscal_DeveRetornarNotaFechadaEPermitirDownload()
    {
        var client = factory.CreateClient();
        var post = await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao());
        var criada = await post.Content.ReadFromJsonAsync<NotaFiscalResponse>();

        var processamento = await client.PostAsync($"/api/notas-fiscais/{criada!.Id}/processar", null);
        var resultado = await processamento.Content.ReadFromJsonAsync<ResultadoProcessamentoNotaFiscalResponse>();
        var arquivo = await client.GetAsync($"/api/notas-fiscais/{criada.Id}/arquivo");

        Assert.Equal(HttpStatusCode.OK, processamento.StatusCode);
        Assert.NotNull(resultado);
        Assert.Equal("Fechada", resultado.NotaFiscal.Status);
        Assert.Equal($"NF-{resultado.NotaFiscal.Numero:D6}.txt", resultado.NomeArquivo);
        Assert.Equal(HttpStatusCode.OK, arquivo.StatusCode);
        Assert.Equal("text/plain", arquivo.Content.Headers.ContentType?.MediaType);
        Assert.Contains("NOTA FISCAL", await arquivo.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ProcessarNotaFiscal_DeveRetornarConflictQuandoSaldoForInsuficiente()
    {
        factory.InventoryClient.SaldoInsuficiente = true;
        var client = factory.CreateClient();
        var post = await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao());
        var criada = await post.Content.ReadFromJsonAsync<NotaFiscalResponse>();

        var resposta = await client.PostAsync($"/api/notas-fiscais/{criada!.Id}/processar", null);
        var erro = await resposta.Content.ReadFromJsonAsync<ErroHttpResponse>();

        factory.InventoryClient.SaldoInsuficiente = false;
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal("SALDO_INSUFICIENTE", erro?.Codigo);

        var detalhe = await client.GetFromJsonAsync<NotaFiscalResponse>($"/api/notas-fiscais/{criada.Id}");
        Assert.Equal("Aberta", detalhe!.Status);
    }

    [Fact]
    public async Task ProcessarNotaFiscal_DeveRetornarServiceUnavailableQuandoInventoryEstiverIndisponivel()
    {
        factory.InventoryClient.Indisponivel = true;
        var client = factory.CreateClient();
        var post = await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao());
        var criada = await post.Content.ReadFromJsonAsync<NotaFiscalResponse>();

        var resposta = await client.PostAsync($"/api/notas-fiscais/{criada!.Id}/processar", null);
        var erro = await resposta.Content.ReadFromJsonAsync<ErroHttpResponse>();

        factory.InventoryClient.Indisponivel = false;
        Assert.Equal(HttpStatusCode.ServiceUnavailable, resposta.StatusCode);
        Assert.Equal("INVENTORY_SERVICE_INDISPONIVEL", erro?.Codigo);
    }

    [Fact]
    public async Task DownloadNotaFiscal_DeveRejeitarNotaAberta()
    {
        var client = factory.CreateClient();
        var post = await client.PostAsJsonAsync("/api/notas-fiscais", CriarRequestPadrao());
        var criada = await post.Content.ReadFromJsonAsync<NotaFiscalResponse>();

        var resposta = await client.GetAsync($"/api/notas-fiscais/{criada!.Id}/arquivo");
        var erro = await resposta.Content.ReadFromJsonAsync<ErroHttpResponse>();

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal("NOTA_FISCAL_NAO_PROCESSADA", erro?.Codigo);
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
