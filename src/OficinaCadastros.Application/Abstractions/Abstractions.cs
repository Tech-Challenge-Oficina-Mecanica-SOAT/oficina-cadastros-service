using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<Cliente?> ObterPorCpfAsync(string cpf, CancellationToken ct);
    Task AdicionarAsync(Cliente cliente, CancellationToken ct);
}

public interface IVeiculoRepository
{
    Task<Veiculo?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<Veiculo?> ObterPorPlacaAsync(string placaNormalizada, CancellationToken ct);
    Task AdicionarAsync(Veiculo veiculo, CancellationToken ct);
}
