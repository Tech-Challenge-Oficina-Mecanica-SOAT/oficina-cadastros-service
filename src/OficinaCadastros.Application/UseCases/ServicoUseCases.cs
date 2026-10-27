using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.UseCases;

public class ServicoUseCases
{
    private readonly IServicoRepository _servicos;
    private readonly IUnitOfWork _uow;

    public ServicoUseCases(IServicoRepository servicos, IUnitOfWork uow)
    {
        _servicos = servicos;
        _uow = uow;
    }

    public async Task<ServicoDto> CriarAsync(CriarServicoRequest request, CancellationToken ct = default)
    {
        var servico = new Servico(request.Nome, request.PrecoUnitario, request.TempoEstimadoHoras);
        await _servicos.AdicionarAsync(servico, ct);
        await _uow.SaveChangesAsync(ct);
        return ServicoDto.From(servico);
    }

    public async Task<ServicoDto> BuscarPorIdAsync(Guid id, CancellationToken ct = default)
        => ServicoDto.From(await ObterOuFalharAsync(id, ct));

    public async Task<ServicoDto> AtualizarAsync(Guid id, AtualizarServicoRequest request, CancellationToken ct = default)
    {
        var servico = await ObterOuFalharAsync(id, ct);
        servico.Atualizar(request.Nome, request.PrecoUnitario, request.TempoEstimadoHoras);
        await _uow.SaveChangesAsync(ct);
        return ServicoDto.From(servico);
    }

    private async Task<Servico> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await _servicos.ObterPorIdAsync(id, ct)
           ?? throw new NotFoundException("SERVICO_NAO_ENCONTRADO", "Serviço não encontrado.");
}
