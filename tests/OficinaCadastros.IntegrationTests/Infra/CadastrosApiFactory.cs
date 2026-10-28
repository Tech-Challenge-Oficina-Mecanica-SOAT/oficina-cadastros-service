using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;
using Testcontainers.PostgreSql;

namespace OficinaCadastros.IntegrationTests.Infra;

/// <summary>
/// Publicador falso: guarda em memória o que o outbox tentaria enviar ao SNS e permite simular
/// falhas transitórias para um cliente específico (sem afetar os eventos de outros testes).
/// </summary>
public class FakeEventPublisher : IEventPublisher
{
    private readonly ConcurrentDictionary<string, int> _falhasRestantes = new();

    public ConcurrentQueue<OutboxEvent> Published { get; } = new();

    /// <summary>Faz as próximas <paramref name="vezes"/> publicações do evento desse cliente falharem.</summary>
    public void FalharPara(Guid clienteId, int vezes) => _falhasRestantes[clienteId.ToString()] = vezes;

    public Task PublishAsync(OutboxEvent evento, CancellationToken ct)
    {
        foreach (var (chave, restantes) in _falhasRestantes)
        {
            if (restantes > 0 && evento.Payload.Contains(chave))
            {
                _falhasRestantes[chave] = restantes - 1;
                throw new InvalidOperationException("Falha simulada na publicação.");
            }
        }

        Published.Enqueue(evento);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Sobe a API inteira em memória contra um PostgreSQL real (container), aplicando as migrations.
/// Um único container é compartilhado por todos os testes (ver CollectionDefinition abaixo).
/// </summary>
public class CadastrosApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("oficina_cadastros_test")
        .Build();

    public FakeEventPublisher Publisher { get; } = new();

    Task IAsyncLifetime.InitializeAsync() => _db.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => _db.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseSetting vale também para o que o Program.cs lê antes de montar o host.
        builder.UseSetting("ConnectionStrings:DefaultConnection", _db.GetConnectionString());
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Messaging:ClienteDesativadoTopicArn", "");
        builder.UseSetting("Messaging:PollIntervalSeconds", "1");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEventPublisher>();
            services.AddSingleton<IEventPublisher>(Publisher);
        });
    }
}

[CollectionDefinition("api")]
public class ApiCollection : ICollectionFixture<CadastrosApiFactory> { }
