using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class FakeOutboxRepository : IOutboxRepository
{
    public List<OutboxEvent> Itens { get; } = new();

    public Task AdicionarAsync(OutboxEvent evento, CancellationToken ct)
    {
        Itens.Add(evento);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxEvent>> ObterPendentesAsync(int max, int maxTentativas, CancellationToken ct)
    {
        IReadOnlyList<OutboxEvent> pendentes = Itens
            .Where(e => e.PublishedAt == null && e.Attempts < maxTentativas)
            .OrderBy(e => e.OccurredAt)
            .Take(max)
            .ToList();
        return Task.FromResult(pendentes);
    }
}
