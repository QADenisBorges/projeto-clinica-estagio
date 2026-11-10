using GestaoClinica.Api.Middlewares;
using GestaoClinica.Domain.Services;
using GestaoClinica.Repository.EF;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Corpo que nem chega ao service (JSON malformado, vazio ou com tipo errado)
// também responde no formato { "mensagem": ... } do resto da API.
builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    options.InvalidModelStateResponseFactory = _ =>
        new BadRequestObjectResult(new { mensagem = "Corpo da requisição inválido." }));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco e repositórios: o registro fica no próprio projeto Repository.EF (classe DependencyInjection).
builder.Services.AddRepository();

builder.Services.AddScoped<ClinicaService>();

var app = builder.Build();

// Popula o banco antes de atender a primeira requisição.
app.Services.PopularDadosIniciais();

// Precisa ser o primeiro da fila para capturar erros de tudo que vem depois.
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

// Quem abrir o endereço da API no navegador cai direto no Swagger.
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapControllers();

app.Run();
