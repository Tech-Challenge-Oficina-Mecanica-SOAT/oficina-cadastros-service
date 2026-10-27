using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.UseCases;

public class VeiculoUseCases
{
    private readonly IVeiculoRepository _veiculos;
    private readonly IClienteRepository _clientes;
    private readonly IUnitOfWork _uow;

    public VeiculoUseCases(IVeiculoRepository veiculos, IClienteRepository clientes, IUnitOfWork uow)
    {
        _veiculos = veiculos;
        _clientes = clientes;
        _uow = uow;
    }

    public async Task<VeiculoDto> CriarAsync(CriarVeiculoRequest request, CancellationToken ct = default)
    {
        if (await _clientes.ObterPorIdAsync(request.ClienteId, ct) is null)
            throw new NotFoundException("CLIENTE_NAO_ENCONTRADO", "Cliente informado não existe.");

        var veiculo = new Veiculo(request.Placa, request.Modelo, request.Marca, request.Ano, request.ClienteId);

        if (await _veiculos.ObterPorPlacaAsync(veiculo.Placa, ct) is not null)
            throw new ConflictException("PLACA_JA_CADASTRADA", "Já existe um veículo com esta placa.");

        await _veiculos.AdicionarAsync(veiculo, ct);
        await _uow.SaveChangesAsync(ct);
        return VeiculoDto.From(veiculo);
    }

    public async Task<VeiculoDto> BuscarPorIdAsync(Guid id, CancellationToken ct = default)
        => VeiculoDto.From(await ObterOuFalharAsync(id, ct));

    public async Task<VeiculoDto> BuscarPorPlacaAsync(string placa, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(placa))
            throw new ArgumentException("Placa é obrigatória.");

        var veiculo = await _veiculos.ObterPorPlacaAsync(Veiculo.NormalizarPlaca(placa), ct)
            ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");
        return VeiculoDto.From(veiculo);
    }

    public async Task<VeiculoDto> AtualizarAsync(Guid id, AtualizarVeiculoRequest request, CancellationToken ct = default)
    {
        var veiculo = await ObterOuFalharAsync(id, ct);
        veiculo.Atualizar(request.Modelo, request.Marca, request.Ano);
        await _uow.SaveChangesAsync(ct);
        return VeiculoDto.From(veiculo);
    }

    private async Task<Veiculo> ObterOuFalharAsync(Guid id, CancellationToken ct)
        => await _veiculos.ObterPorIdAsync(id, ct)
           ?? throw new NotFoundException("VEICULO_NAO_ENCONTRADO", "Veículo não encontrado.");
}
