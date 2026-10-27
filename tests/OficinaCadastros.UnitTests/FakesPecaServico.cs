using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class FakePecaRepository : IPecaRepository
{
    public List<Peca> Itens { get; } = new();

    public Task<Peca?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(p => p.Id == id));

    public Task<Peca?> ObterPorCodigoAsync(string codigoNormalizado, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(p => p.Codigo == codigoNormalizado));

    public Task<IReadOnlyList<Peca>> ListarAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<Peca>>(Itens.ToList());

    public Task AdicionarAsync(Peca peca, CancellationToken ct)
    {
        Itens.Add(peca);
        return Task.CompletedTask;
    }
}

public class FakeServicoRepository : IServicoRepository
{
    public List<Servico> Itens { get; } = new();

    public Task<Servico?> ObterPorIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(Itens.FirstOrDefault(s => s.Id == id));

    public Task AdicionarAsync(Servico servico, CancellationToken ct)
    {
        Itens.Add(servico);
        return Task.CompletedTask;
    }
}
