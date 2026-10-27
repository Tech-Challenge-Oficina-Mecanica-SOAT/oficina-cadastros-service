using Microsoft.EntityFrameworkCore;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Persistence.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly CadastrosDbContext _db;

    public ServicoRepository(CadastrosDbContext db) => _db = db;

    public Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => _db.Servicos.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task AdicionarAsync(Servico servico, CancellationToken ct)
    {
        _db.Servicos.Add(servico);
        return Task.CompletedTask;
    }
}
