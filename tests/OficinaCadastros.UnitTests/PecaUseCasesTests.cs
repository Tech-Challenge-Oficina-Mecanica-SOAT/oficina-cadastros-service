using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.UnitTests;

public class PecaUseCasesTests
{
    private readonly FakePecaRepository _repo = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly PecaUseCases _sut;

    public PecaUseCasesTests() => _sut = new PecaUseCases(_repo, _uow);

    private static CriarPecaRequest Req(string codigo = "FLT-OLEO-001", decimal preco = 35.90m, int estoque = 10) => new()
    {
        Codigo = codigo,
        Nome = "Filtro de óleo",
        PrecoUnitario = preco,
        Estoque = estoque
    };

    [Fact]
    public async Task Criar_ComDadosValidos_NormalizaCodigoEPersiste()
    {
        var dto = await _sut.CriarAsync(Req(" flt-oleo-001 "));

        Assert.Equal("FLT-OLEO-001", dto.Codigo);
        Assert.Equal(35.90m, dto.PrecoUnitario);
        Assert.Equal(10, dto.Estoque);
        Assert.Single(_repo.Itens);
        Assert.Equal(1, _uow.Saves);
    }

    [Fact]
    public async Task Criar_CodigoDuplicadoEmOutraCaixa_LancaConflict()
    {
        await _sut.CriarAsync(Req("FLT-OLEO-001"));

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CriarAsync(Req("flt-oleo-001")));
        Assert.Equal("CODIGO_JA_CADASTRADO", ex.Code);
        Assert.Single(_repo.Itens);
    }

    [Fact]
    public async Task Criar_PrecoNegativo_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CriarAsync(Req(preco: -1m)));
        Assert.Empty(_repo.Itens);
    }

    [Fact]
    public async Task Criar_EstoqueNegativo_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.CriarAsync(Req(estoque: -1)));
        Assert.Empty(_repo.Itens);
    }

    [Fact]
    public async Task Criar_ComEstoqueZero_EhPermitido()
    {
        var dto = await _sut.CriarAsync(Req(estoque: 0));

        Assert.Equal(0, dto.Estoque);
    }

    [Fact]
    public async Task Listar_RetornaTodasAsPecas()
    {
        await _sut.CriarAsync(Req("A-1"));
        await _sut.CriarAsync(Req("B-2"));

        var lista = await _sut.ListarAsync();

        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public async Task Listar_SemPecas_RetornaListaVazia()
    {
        Assert.Empty(await _sut.ListarAsync());
    }

    [Fact]
    public async Task BuscarPorId_Existente_RetornaPeca()
    {
        var criada = await _sut.CriarAsync(Req());

        var dto = await _sut.BuscarPorIdAsync(criada.Id);

        Assert.Equal(criada.Id, dto.Id);
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_LancaNotFound()
    {
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.BuscarPorIdAsync(Guid.NewGuid()));
        Assert.Equal("PECA_NAO_ENCONTRADA", ex.Code);
    }

    [Fact]
    public async Task Atualizar_AlteraNomePrecoEEstoque_MantemCodigo()
    {
        var criada = await _sut.CriarAsync(Req());

        var dto = await _sut.AtualizarAsync(criada.Id, new AtualizarPecaRequest
        {
            Nome = "Filtro de óleo premium",
            PrecoUnitario = 49.90m,
            Estoque = 3
        });

        Assert.Equal("Filtro de óleo premium", dto.Nome);
        Assert.Equal(49.90m, dto.PrecoUnitario);
        Assert.Equal(3, dto.Estoque);
        Assert.Equal("FLT-OLEO-001", dto.Codigo);
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AtualizarAsync(Guid.NewGuid(), new AtualizarPecaRequest { Nome = "X", PrecoUnitario = 1, Estoque = 1 }));
    }

    [Fact]
    public async Task Atualizar_EstoqueNegativo_LancaArgumentException()
    {
        var criada = await _sut.CriarAsync(Req());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.AtualizarAsync(criada.Id, new AtualizarPecaRequest { Nome = "X", PrecoUnitario = 1, Estoque = -5 }));
    }
}
