using Microsoft.EntityFrameworkCore;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Persistence.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly CadastrosDbContext _db;

    public OutboxRepository(CadastrosDbContext db) => _db = db;

    public Task AdicionarAsync(OutboxEvent evento, CancellationToken ct)
    {
        _db.OutboxEvents.Add(evento);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<OutboxEvent>> ObterPendentesAsync(int max, int maxTentativas, CancellationToken ct)
        => await _db.OutboxEvents
            .Where(e => e.PublishedAt == null && e.Attempts < maxTentativas)
            .OrderBy(e => e.OccurredAt)
            .Take(max)
            .ToListAsync(ct);
}
