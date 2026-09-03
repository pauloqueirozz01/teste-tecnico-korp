using KorpTeste.InventoryService.Application.Interfaces;
using KorpTeste.InventoryService.Application.Services;
using KorpTeste.InventoryService.Application.DTOs;
using KorpTeste.InventoryService.Infrastructure.Persistence;
using KorpTeste.InventoryService.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

const string CorsPolicyName = "FrontendDesenvolvimento";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var mensagens = context.ModelState
            .Where(item => item.Value?.Errors.Count > 0)
            .SelectMany(item => item.Value!.Errors.Select(erro => erro.ErrorMessage))
            .Where(mensagem => !string.IsNullOrWhiteSpace(mensagem))
            .ToArray();

        return new BadRequestObjectResult(new ErroHttpResponse(
            "REQUISICAO_INVALIDA",
            mensagens.Length > 0
                ? string.Join(" ", mensagens)
                : "A requisição enviada é inválida.",
            StatusCodes.Status400BadRequest));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProdutoService, ProdutoService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
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
