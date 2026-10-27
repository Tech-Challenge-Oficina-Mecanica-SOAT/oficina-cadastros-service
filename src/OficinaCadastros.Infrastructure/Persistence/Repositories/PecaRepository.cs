using Microsoft.EntityFrameworkCore;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Persistence.Repositories;

public class PecaRepository : IPecaRepository
{
    private readonly CadastrosDbContext _db;

    public PecaRepository(CadastrosDbContext db) => _db = db;

    public Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => _db.Pecas.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Peca?> ObterPorCodigoAsync(string codigoNormalizado, CancellationToken ct)
        => _db.Pecas.FirstOrDefaultAsync(p => p.Codigo == codigoNormalizado, ct);

    public async Task<IReadOnlyList<Peca>> ListarAsync(CancellationToken ct)
        => await _db.Pecas.AsNoTracking().OrderBy(p => p.Nome).ToListAsync(ct);

    public Task AdicionarAsync(Peca peca, CancellationToken ct)
    {
        _db.Pecas.Add(peca);
        return Task.CompletedTask;
    }
}
