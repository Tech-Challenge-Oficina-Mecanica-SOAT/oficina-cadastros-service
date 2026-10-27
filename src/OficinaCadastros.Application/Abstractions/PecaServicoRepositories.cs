using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Abstractions;

public interface IPecaRepository
{
    Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task<Peca?> ObterPorCodigoAsync(string codigoNormalizado, CancellationToken ct);
    Task<IReadOnlyList<Peca>> ListarAsync(CancellationToken ct);
    Task AdicionarAsync(Peca peca, CancellationToken ct);
}

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct);
    Task AdicionarAsync(Servico servico, CancellationToken ct);
}
