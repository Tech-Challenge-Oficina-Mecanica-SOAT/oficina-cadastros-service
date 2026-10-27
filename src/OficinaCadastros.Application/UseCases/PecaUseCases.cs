using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.UseCases;

public class PecaUseCases
{
    private readonly IPecaRepository _pecas;
    private readonly IUnitOfWork _uow;

    public PecaUseCases(IPecaRepository pecas, IUnitOfWork uow)
    {
        _pecas = pecas;
        _uow = uow;
    }

    public async Task<PecaDto> CriarAsync(CriarPecaRequest request, CancellationToken ct = default)
    {
        var peca = new Peca(request.Codigo, request.Nome, request.PrecoUnitario, request.Estoque);

        if (await _pecas.ObterPorCodigoAsync(peca.Codigo, ct) is not null)
            throw new ConflictException("CODIGO_JA_CADASTRADO", "Já existe uma peça com este código.");

        await _pecas.AdicionarAsync(peca, ct);
        await _uow.SaveChangesAsync(ct);
        return PecaDto.From(peca);
    }

    public async Task<IReadOnlyList<PecaDto>> ListarAsync(CancellationToken ct = default)
    {
        var pecas = await _pecas.ListarAsync(ct);
        return pecas.Select(PecaDto.From).ToList();
    }

    public async Task<PecaDto> BuscarPorIdAsync(Guid id, CancellationToken ct = default)
        => PecaDto.From(await ObterOuFalharAsync(id, ct));

    public async Task<PecaDto> AtualizarAsync(Guid id, AtualizarPecaRequest request, CancellationToken ct = default)
    {
        var peca = await ObterOuFalharAsync(id, ct);
        peca.Atualizar(request.Nome, request.PrecoUnitario, request.Estoque);
        await _uow.SaveChangesAsync(ct);
        return PecaDto.From(peca);
    }

    private async Task<Peca> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await _pecas.ObterPorIdAsync(id, ct)
           ?? throw new NotFoundException("PECA_NAO_ENCONTRADA", "Peça não encontrada.");
}
