using Microsoft.EntityFrameworkCore;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Persistence.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly CadastrosDbContext _db;

    public ClienteRepository(CadastrosDbContext db) => _db = db;

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => _db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken ct)
        => _db.Clientes.FirstOrDefaultAsync(c => c.Cpf == cpf, ct);

    public Task AdicionarAsync(Cliente cliente, CancellationToken ct)
    {
        _db.Clientes.Add(cliente);
        return Task.CompletedTask;
    }
}
