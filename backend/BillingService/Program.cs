using KorpTeste.BillingService.Application.DTOs;
using KorpTeste.BillingService.Application.Interfaces;
using KorpTeste.BillingService.Application.Services;
using KorpTeste.BillingService.Infrastructure.Persistence;
using KorpTeste.BillingService.Infrastructure.Inventory;
using KorpTeste.BillingService.Infrastructure.Storage;
using KorpTeste.BillingService.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;

const string CorsPolicyName = "FrontendDesenvolvimento";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensagem = context.ModelState
            .SelectMany(item => item.Value?.Errors ?? [])
            .Select(erro => erro.ErrorMessage)
            .FirstOrDefault(mensagemErro => !string.IsNullOrWhiteSpace(mensagemErro))
            ?? "A requisição enviada é inválida.";

        return new BadRequestObjectResult(new ErroHttpResponse("REQUISICAO_INVALIDA", mensagem, StatusCodes.Status400BadRequest));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<INumeradorNotaFiscal, PostgreSqlNumeradorNotaFiscal>();
builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();
builder.Services.AddScoped<IProcessamentoNotaFiscalService, ProcessamentoNotaFiscalService>();
builder.Services.Configure<ArmazenamentoNotasOptions>(
    builder.Configuration.GetSection(ArmazenamentoNotasOptions.SectionName));
builder.Services.Configure<InventoryServiceOptions>(
    builder.Configuration.GetSection(InventoryServiceOptions.SectionName));
builder.Services.AddSingleton<IGeradorArquivoNotaFiscal, GeradorArquivoNotaFiscal>();

var inventoryOptions = builder.Configuration
    .GetSection(InventoryServiceOptions.SectionName)
    .Get<InventoryServiceOptions>() ?? new InventoryServiceOptions();

var inventoryBaseUrl = inventoryOptions.BaseUrl
    ?? throw new InvalidOperationException("A URL do InventoryService não está configurada.");
if (string.IsNullOrWhiteSpace(inventoryBaseUrl))
{
    throw new InvalidOperationException("A URL do InventoryService não está configurada.");
}

builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
    client.Timeout = Timeout.InfiniteTimeSpan;
})
.AddResilienceHandler("inventory-service", (pipeline, context) =>
{
    var options = context.ServiceProvider
        .GetRequiredService<IConfiguration>()
        .GetSection(InventoryServiceOptions.SectionName)
        .Get<InventoryServiceOptions>() ?? new InventoryServiceOptions();
    var resilience = options.Resilience;
    var retryLogger = context.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("InventoryService.Resilience");

    if (resilience.RetryCount > 0)
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = resilience.RetryCount,
            Delay = TimeSpan.FromMilliseconds(resilience.RetryBaseDelayMilliseconds),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = static args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome)),
            OnRetry = args =>
            {
                retryLogger.LogWarning(
                    "Tentativa adicional de comunicação com InventoryService. Tentativa {Tentativa}. Motivo: {Motivo}.",
                    args.AttemptNumber + 1,
                    args.Outcome.Exception?.GetType().Name ?? args.Outcome.Result?.StatusCode.ToString());

                return ValueTask.CompletedTask;
            }
        });
    }

    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        FailureRatio = resilience.CircuitBreakerFailureRatio,
        MinimumThroughput = resilience.CircuitBreakerMinimumThroughput,
        SamplingDuration = TimeSpan.FromSeconds(resilience.CircuitBreakerSamplingSeconds),
        BreakDuration = TimeSpan.FromSeconds(resilience.CircuitBreakerBreakSeconds),
        ShouldHandle = static args => ValueTask.FromResult(HttpClientResiliencePredicates.IsTransient(args.Outcome))
    });

    pipeline.AddTimeout(TimeSpan.FromSeconds(options.TimeoutSeconds));
});
builder.Services.AddHealthChecks()
    .AddCheck<PostgreSqlHealthCheck>("postgresql");

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicyName);
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
