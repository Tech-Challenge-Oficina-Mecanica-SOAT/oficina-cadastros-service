using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OficinaCadastros.API.Middleware;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Application.UseCases;
using OficinaCadastros.Infrastructure.Persistence;
using OficinaCadastros.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validação do corpo no mesmo formato do contrato: { code, message }
        options.InvalidModelStateResponseFactory = ctx =>
        {
            var mensagens = ctx.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Corpo da requisição inválido." : e.ErrorMessage)
                .Distinct();
            return new BadRequestObjectResult(new ErrorResponse("REQUISICAO_INVALIDA", string.Join(" ", mensagens)));
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CadastrosDbContext>(opt =>
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.MigrationsAssembly("OficinaCadastros.Infrastructure")));

builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CadastrosDbContext>());
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<ClienteUseCases>();
builder.Services.AddScoped<VeiculoUseCases>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CadastrosDbContext>("db", tags: new[] { "ready" });

var app = builder.Build();

// Aplica as migrations ao subir (usado no docker compose local; em K8s será um job/init).
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<CadastrosDbContext>().Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// live: processo de pé. ready: processo + banco acessível.
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });

app.Run();

public partial class Program { }
