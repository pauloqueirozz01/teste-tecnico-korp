using System.Net;
using System.Net.Http.Json;
using InventoryService.Tests.TestHelpers;
using KorpTeste.InventoryService.Application.DTOs;

namespace InventoryService.Tests;

public class InventoryApiTests : IClassFixture<InventoryApiFactory>
{
    private readonly HttpClient _client;

    public InventoryApiTests(InventoryApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostProdutos_DeveCriarProdutoERetornarCreated()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/produtos",
            new CriarProdutoRequest("PROD-API-001", "Produto API", 10));

        var produto = await response.Content.ReadFromJsonAsync<ProdutoResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(produto);
        Assert.Equal("PROD-API-001", produto.Codigo);
    }

    [Fact]
    public async Task GetProdutos_DeveListarProdutos()
    {
        await _client.PostAsJsonAsync(
            "/api/produtos",
            new CriarProdutoRequest("PROD-API-002", "Produto API", 10));

        var produtos = await _client.GetFromJsonAsync<IReadOnlyCollection<ProdutoResponse>>("/api/produtos");

        Assert.NotNull(produtos);
        Assert.Contains(produtos, produto => produto.Codigo == "PROD-API-002");
    }

    [Fact]
    public async Task GetProdutoPorId_DeveRetornarProdutoExistente()
    {
        var criarResponse = await _client.PostAsJsonAsync(
            "/api/produtos",
            new CriarProdutoRequest("PROD-API-003", "Produto API", 10));
        var criado = await criarResponse.Content.ReadFromJsonAsync<ProdutoResponse>();

        var produto = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/produtos/{criado!.Id}");

        Assert.NotNull(produto);
        Assert.Equal(criado.Id, produto.Id);
    }

    [Fact]
    public async Task GetProdutoPorId_DeveRetornarNotFoundQuandoProdutoNaoExistir()
    {
        var response = await _client.GetAsync($"/api/produtos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ValidarEstoque_DeveRetornarOkQuandoSaldoForSuficiente()
    {
        var produto = await CriarProdutoAsync("PROD-API-004", 10);

        var response = await _client.PostAsJsonAsync(
            "/api/estoque/validar",
            new ValidarEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConsumirEstoque_DeveRetornarOkEAtualizarSaldo()
    {
        var produto = await CriarProdutoAsync("PROD-API-005", 10);

        var response = await _client.PostAsJsonAsync(
            "/api/estoque/consumir",
            new ConsumirEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]));

        var atualizado = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/produtos/{produto.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(8, atualizado!.Saldo);
    }

    [Fact]
    public async Task ConsumirEstoque_DeveSerIdempotenteQuandoHeaderForRepetido()
    {
        var produto = await CriarProdutoAsync("PROD-API-IDEMP-001", 10);
        var request = new ConsumirEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]);
        using var primeira = new HttpRequestMessage(HttpMethod.Post, "/api/estoque/consumir")
        {
            Content = JsonContent.Create(request)
        };
        primeira.Headers.Add("Idempotency-Key", "consumo-api-001");
        using var segunda = new HttpRequestMessage(HttpMethod.Post, "/api/estoque/consumir")
        {
            Content = JsonContent.Create(request)
        };
        segunda.Headers.Add("Idempotency-Key", "consumo-api-001");

        var primeiraResposta = await _client.SendAsync(primeira);
        var segundaResposta = await _client.SendAsync(segunda);
        var atualizado = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/produtos/{produto.Id}");

        Assert.Equal(HttpStatusCode.OK, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segundaResposta.StatusCode);
        Assert.Equal(8, atualizado!.Saldo);
    }

    [Fact]
    public async Task ConsumirEstoque_DeveRetornarConflictQuandoSaldoForInsuficiente()
    {
        var produto = await CriarProdutoAsync("PROD-API-006", 1);

        var response = await _client.PostAsJsonAsync(
            "/api/estoque/consumir",
            new ConsumirEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]));

        var atualizado = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/produtos/{produto.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, atualizado!.Saldo);
    }

    [Fact]
    public async Task ReporEstoque_DeveRetornarOkEAtualizarSaldo()
    {
        var produto = await CriarProdutoAsync("PROD-API-007", 8);

        var response = await _client.PostAsJsonAsync(
            "/api/estoque/repor",
            new ReporEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]));

        var atualizado = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/produtos/{produto.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(10, atualizado!.Saldo);
    }

    [Fact]
    public async Task ReporEstoque_DeveSerIdempotenteQuandoHeaderForRepetido()
    {
        var produto = await CriarProdutoAsync("PROD-API-IDEMP-002", 8);
        var request = new ReporEstoqueRequest([new ItemEstoqueRequest(produto.Id, 2)]);
        using var primeira = new HttpRequestMessage(HttpMethod.Post, "/api/estoque/repor")
        {
            Content = JsonContent.Create(request)
        };
        primeira.Headers.Add("Idempotency-Key", "compensacao-api-001");
        using var segunda = new HttpRequestMessage(HttpMethod.Post, "/api/estoque/repor")
        {
            Content = JsonContent.Create(request)
        };
        segunda.Headers.Add("Idempotency-Key", "compensacao-api-001");

        var primeiraResposta = await _client.SendAsync(primeira);
        var segundaResposta = await _client.SendAsync(segunda);
        var atualizado = await _client.GetFromJsonAsync<ProdutoResponse>($"/api/produtos/{produto.Id}");

        Assert.Equal(HttpStatusCode.OK, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.OK, segundaResposta.StatusCode);
        Assert.Equal(10, atualizado!.Saldo);
    }

    private async Task<ProdutoResponse> CriarProdutoAsync(string codigo, int saldo)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/produtos",
            new CriarProdutoRequest(codigo, $"Produto {codigo}", saldo));

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProdutoResponse>())!;
    }
}
