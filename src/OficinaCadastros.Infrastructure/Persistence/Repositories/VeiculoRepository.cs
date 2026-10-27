using Microsoft.EntityFrameworkCore;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Persistence.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly CadastrosDbContext _db;

    public VeiculoRepository(CadastrosDbContext db) => _db = db;

    public Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => _db.Veiculos.FirstOrDefaultAsync(v => v.Id == id, ct);

    public Task<Veiculo?> ObterPorPlacaAsync(string placaNormalizada, CancellationToken ct)
        => _db.Veiculos.FirstOrDefaultAsync(v => v.Placa == placaNormalizada, ct);

    public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct)
    {
        _db.Veiculos.Add(veiculo);
        return Task.CompletedTask;
    }
}
