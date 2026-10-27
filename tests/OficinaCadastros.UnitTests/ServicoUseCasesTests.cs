using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.UnitTests;

public class ServicoUseCasesTests
{
    private readonly FakeServicoRepository _repo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly ServicoUseCases _sut;

    public ServicoUseCasesTests() => _sut = new ServicoUseCases(_repo, _uow);

    private static CriarServicoRequest Req(decimal preco = 80m, decimal tempo = 0.5m) => new()
    {
        Nome = "Troca de óleo",
        PrecoUnitario = preco,
        TempoEstimadoHoras = tempo
    };

    [Fact]
    public async Task Criar_ComDadosValidos_PersisteERetornaDto()
    {
        var dto = await _sut.CriarAsync(Req());

        Assert.Equal("Troca de óleo", dto.Nome);
        Assert.Equal(80m, dto.PrecoUnitario);
        Assert.Equal(0.5m, dto.TempoEstimadoHoras);
        Assert.Single(_repo.Itens);
        Assert.Equal(1, _uow.Saves);
    }

    [Fact]
    public async Task Criar_PrecoNegativo_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CriarAsync(Req(preco: -10m)));
        Assert.Empty(_repo.Itens);
    }

    [Fact]
    public async Task Criar_TempoNegativo_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CriarAsync(Req(tempo: -1m)));
        Assert.Empty(_repo.Itens);
    }

    [Fact]
    public async Task BuscarPorId_Existente_RetornaServico()
    {
        var criado = await _sut.CriarAsync(Req());

        var dto = await _sut.BuscarPorIdAsync(criado.Id);

        Assert.Equal(criado.Id, dto.Id);
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_LancaNotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.BuscarPorIdAsync(Guid.NewGuid()));
        Assert.Equal("SERVICO_NAO_ENCONTRADO", ex.Code);
    }

    [Fact]
    public async Task Atualizar_AlteraTodosOsCampos()
    {
        var criado = await _sut.CriarAsync(Req());

        var dto = await _sut.AtualizarAsync(criado.Id, new AtualizarServicoRequest
        {
            Nome = "Alinhamento",
            PrecoUnitario = 150m,
            TempoEstimadoHoras = 1.5m
        });

        Assert.Equal("Alinhamento", dto.Nome);
        Assert.Equal(150m, dto.PrecoUnitario);
        Assert.Equal(1.5m, dto.TempoEstimadoHoras);
        Assert.Equal(criado.Id, dto.Id);
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AtualizarAsync(Guid.NewGuid(), new AtualizarServicoRequest { Nome = "X", PrecoUnitario = 1, TempoEstimadoHoras = 1 }));
    }

    [Fact]
    public async Task Atualizar_PrecoNegativo_LancaArgumentException()
    {
        var criado = await _sut.CriarAsync(Req());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.AtualizarAsync(criado.Id, new AtualizarServicoRequest { Nome = "X", PrecoUnitario = -1, TempoEstimadoHoras = 1 }));
    }
}
