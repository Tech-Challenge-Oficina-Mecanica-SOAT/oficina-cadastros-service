using Microsoft.Extensions.Logging;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Messaging;

/// <summary>Publicador de desenvolvimento: não envia a lugar nenhum, apenas registra o evento no log.</summary>
public class LogEventPublisher : IEventPublisher
{
    private readonly ILogger<LogEventPublisher> _logger;

    public LogEventPublisher(ILogger<LogEventPublisher> logger) => _logger = logger;

    public Task PublishAsync(OutboxEvent evento, CancellationToken ct)
    {
        _logger.LogInformation("[outbox:log] {Tipo} {EventId} {Payload}", evento.Type, evento.Id, evento.Payload);
        return Task.CompletedTask;
    }
}
