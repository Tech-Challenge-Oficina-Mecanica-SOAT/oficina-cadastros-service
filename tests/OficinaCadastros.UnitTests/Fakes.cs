using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class FakeUnitOfWork : IUnitOfWork
{
    public int Saves { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Saves++;
        return Task.FromResult(1);
    }
}

public class FakeClienteRepository : IClienteRepository
{
    public List<Cliente> Itens { get; } = new();

    public Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(c => c.Id == id));

    public Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(c => c.Cpf == cpf));

    public Task AdicionarAsync(Cliente cliente, CancellationToken ct)
    {
        Itens.Add(cliente);
        return Task.CompletedTask;
    }
}

public class FakeVeiculoRepository : IVeiculoRepository
{
    public List<Veiculo> Itens { get; } = new();

    public Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(v => v.Id == id));

    public Task<Veiculo?> ObterPorPlacaAsync(string placaNormalizada, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(v => v.Placa == placaNormalizada));

    public Task AdicionarAsync(Veiculo veiculo, CancellationToken ct)
    {
        Itens.Add(veiculo);
        return Task.CompletedTask;
    }
}
