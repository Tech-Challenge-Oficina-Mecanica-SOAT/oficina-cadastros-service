using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.UnitTests;

public class VeiculoUseCasesTests
{
    private readonly FakeClienteRepository _clientes = new();
    private readonly FakeVeiculoRepository _veiculos = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly VeiculoUseCases _sut;
    private readonly Cliente _cliente = new("Maria da Silva", "12345678901", "maria@email.com", null);

    public VeiculoUseCasesTests()
    {
        _clientes.Itens.Add(_cliente);
        _sut = new VeiculoUseCases(_veiculos, _clientes, _uow);
    }

    private CriarVeiculoRequest Req(string placa = "ABC1D23", Guid? clienteId = null) => new()
    {
        Placa = placa,
        Modelo = "Onix",
        Marca = "Chevrolet",
        Ano = 2021,
        ClienteId = clienteId ?? _cliente.Id
    };

    [Fact]
    public async Task Criar_ComDadosValidos_NormalizaPlacaEPersiste()
    {
        var dto = await _sut.CriarAsync(Req(" abc1d23 "));

        Assert.Equal("ABC1D23", dto.Placa);
        Assert.Equal(_cliente.Id, dto.ClienteId);
        Assert.Single(_veiculos.Itens);
        Assert.Equal(1, _uow.Saves);
    }

    [Fact]
    public async Task Criar_ClienteInexistente_LancaNotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CriarAsync(Req(clienteId: Guid.NewGuid())));

        Assert.Equal("CLIENTE_NAO_ENCONTRADO", ex.Code);
        Assert.Empty(_veiculos.Itens);
    }

    [Fact]
    public async Task Criar_PlacaDuplicadaEmOutraCaixa_LancaConflict()
    {
        await _sut.CriarAsync(Req("ABC1D23"));

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CriarAsync(Req("abc1d23")));
        Assert.Equal("PLACA_JA_CADASTRADA", ex.Code);
        Assert.Single(_veiculos.Itens);
    }

    [Fact]
    public async Task Criar_AnoInvalido_LancaArgumentException()
    {
        var req = Req();
        req.Ano = 1800;

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CriarAsync(req));
    }

    [Fact]
    public async Task BuscarPorPlaca_IgnoraCaixa()
    {
        await _sut.CriarAsync(Req("ABC1D23"));

        var dto = await _sut.BuscarPorPlacaAsync("abc1d23");

        Assert.Equal("ABC1D23", dto.Placa);
    }

    [Fact]
    public async Task BuscarPorPlaca_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.BuscarPorPlacaAsync("ZZZ9Z99"));
    }

    [Fact]
    public async Task BuscarPorId_Existente_RetornaVeiculo()
    {
        var criado = await _sut.CriarAsync(Req());

        var dto = await _sut.BuscarPorIdAsync(criado.Id);

        Assert.Equal(criado.Id, dto.Id);
    }

    [Fact]
    public async Task Atualizar_AlteraModeloMarcaEAno_MantemPlacaECliente()
    {
        var criado = await _sut.CriarAsync(Req());

        var dto = await _sut.AtualizarAsync(criado.Id, new AtualizarVeiculoRequest
        {
            Modelo = "Tracker",
            Marca = "Chevrolet",
            Ano = 2023
        });

        Assert.Equal("Tracker", dto.Modelo);
        Assert.Equal(2023, dto.Ano);
        Assert.Equal("ABC1D23", dto.Placa);
        Assert.Equal(_cliente.Id, dto.ClienteId);
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AtualizarAsync(Guid.NewGuid(), new AtualizarVeiculoRequest { Modelo = "X", Marca = "Y", Ano = 2020 }));
    }

    [Fact]
    public async Task Atualizar_AnoInvalido_LancaArgumentException()
    {
        var criado = await _sut.CriarAsync(Req());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.AtualizarAsync(criado.Id, new AtualizarVeiculoRequest { Modelo = "X", Marca = "Y", Ano = 1500 }));
    }
}
