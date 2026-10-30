using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;
using Testcontainers.PostgreSql;

namespace OficinaCadastros.BDD.Support;

/// <summary>Publicador falso: guarda em memória o que o outbox tentaria enviar ao SNS.</summary>
public class FakeEventPublisher : IEventPublisher
{
    public ConcurrentQueue<OutboxEvent> Published { get; } = new();

    public Task PublishAsync(OutboxEvent evento, CancellationToken ct)
    {
        Published.Enqueue(evento);
        return Task.CompletedTask;
    }
}

/// <summary>A API inteira em memória contra um PostgreSQL real (container), com as migrations aplicadas.</summary>
public class CadastrosApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("oficina_cadastros_bdd")
        .Build();

    public FakeEventPublisher Publisher { get; } = new();

    Task IAsyncLifetime.InitializeAsync() => _db.StartAsync();

    Task IAsyncLifetime.DisposeAsync() => _db.DisposeAsync().AsTask();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
