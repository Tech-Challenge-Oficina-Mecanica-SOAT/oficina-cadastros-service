using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Abstractions;

public interface IOutboxRepository
{
    Task AdicionarAsync(OutboxEvent evento, CancellationToken ct);

    /// <summary>Eventos ainda não publicados e com menos de <paramref name="maxTentativas"/> falhas, do mais antigo ao mais novo.</summary>
    Task<IReadOnlyList<OutboxEvent>> ObterPendentesAsync(int max, int maxTentativas, CancellationToken ct);
}

public interface IEventPublisher
{
    Task PublishAsync(OutboxEvent evento, CancellationToken ct);
}
