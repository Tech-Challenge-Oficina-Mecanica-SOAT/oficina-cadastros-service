using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Infrastructure.Messaging;

namespace OficinaCadastros.API.Messaging;

/// <summary>
/// Lê periodicamente a tabela outbox_events e publica o que ainda está pendente.
/// Entrega "pelo menos uma vez": se o processo cair entre publicar e gravar PublishedAt,
/// o evento é reenviado. Os consumers já são idempotentes por eventId (contrato de eventos).
/// </summary>
public class OutboxPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IEventPublisher _publisher;
    private readonly MessagingOptions _options;
    private readonly ILogger<OutboxPublisherService> _logger;

    public OutboxPublisherService(
        IServiceScopeFactory scopes,
        IEventPublisher publisher,
        MessagingOptions options,
        ILogger<OutboxPublisherService> logger)
    {
        _scopes = scopes;
        _publisher = publisher;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var publicados = await ProcessarLoteAsync(stoppingToken);
                if (publicados >= _options.BatchSize) continue; // ainda há fila: não espera
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no ciclo do outbox");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <returns>Quantos eventos foram publicados com sucesso neste lote.</returns>
    private async Task<int> ProcessarLoteAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var pendentes = await outbox.ObterPendentesAsync(_options.BatchSize, _options.MaxAttempts, ct);
        var publicados = 0;

        foreach (var evento in pendentes)
        {
            try
            {
                await _publisher.PublishAsync(evento, ct);
                evento.MarcarPublicado(DateTime.UtcNow);
                publicados++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                evento.RegistrarFalha(ex.Message);
                _logger.LogWarning(ex, "Falha ao publicar evento {EventId} (tentativa {Tentativa})", evento.Id, evento.Attempts);
            }
        }

        if (pendentes.Count > 0)
            await uow.SaveChangesAsync(ct);

        return publicados;
    }
}
