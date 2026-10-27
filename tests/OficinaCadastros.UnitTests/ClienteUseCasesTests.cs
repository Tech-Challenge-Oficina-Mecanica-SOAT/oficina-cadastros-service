using System.Text.Json;
using System.Text.RegularExpressions;
using OficinaCadastros.Application.Common;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Application.UseCases;

namespace OficinaCadastros.UnitTests;

public class ClienteUseCasesTests
{
    private readonly FakeClienteRepository _repo = new();
    private readonly FakeOutboxRepository _outbox = new();
    private readonly FakeUnitOfWork _uow = new();
    private readonly ClienteUseCases _sut;

    public ClienteUseCasesTests() => _sut = new ClienteUseCases(_repo, _outbox, _uow);

    private static CriarClienteRequest Req(string cpf = "12345678901") => new()
    {
        Nome = "Maria da Silva",
        Cpf = cpf,
        Email = "maria@email.com",
        Telefone = "62999990000"
    };

    [Fact]
    public async Task Criar_ComDadosValidos_PersisteERetornaDto()
    {
        var dto = await _sut.CriarAsync(Req());

        Assert.Equal("12345678901", dto.Cpf);
        Assert.True(dto.Ativo);
        Assert.Single(_repo.Itens);
        Assert.Equal(1, _uow.Saves);
    }

    [Fact]
    public async Task Criar_CpfDuplicado_LancaConflict()
    {
        await _sut.CriarAsync(Req());

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.CriarAsync(Req()));
        Assert.Equal("CPF_JA_CADASTRADO", ex.Code);
        Assert.Single(_repo.Itens);
    }

    [Fact]
    public async Task BuscarPorCpf_Existente_RetornaCliente()
    {
        await _sut.CriarAsync(Req());

        var dto = await _sut.BuscarPorCpfAsync("12345678901");

        Assert.Equal("Maria da Silva", dto.Nome);
    }

    [Fact]
    public async Task BuscarPorCpf_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.BuscarPorCpfAsync("00000000000"));
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890a")]
    [InlineData("")]
    public async Task BuscarPorCpf_FormatoInvalido_LancaArgumentException(string cpf)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.BuscarPorCpfAsync(cpf));
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.BuscarPorIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Atualizar_AlteraNomeEmailETelefone_MantemCpf()
    {
        var criado = await _sut.CriarAsync(Req());

        var dto = await _sut.AtualizarAsync(criado.Id, new AtualizarClienteRequest
        {
            Nome = "Maria Souza",
            Email = "maria.souza@email.com",
            Telefone = null
        });

        Assert.Equal("Maria Souza", dto.Nome);
        Assert.Equal("maria.souza@email.com", dto.Email);
        Assert.Null(dto.Telefone);
        Assert.Equal("12345678901", dto.Cpf);
    }

    [Fact]
    public async Task Atualizar_Inexistente_LancaNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.AtualizarAsync(Guid.NewGuid(), new AtualizarClienteRequest { Nome = "X", Email = "x@x.com" }));
    }

    [Fact]
    public async Task Desativar_MarcaComoInativoESalva()
    {
        var criado = await _sut.CriarAsync(Req());

        await _sut.DesativarAsync(criado.Id, "inadimplencia");

        var dto = await _sut.BuscarPorIdAsync(criado.Id);
        Assert.False(dto.Ativo);
        Assert.Equal(2, _uow.Saves); // criar + desativar
    }

    [Fact]
    public async Task Desativar_SemMotivo_Funciona()
    {
        var criado = await _sut.CriarAsync(Req());

        await _sut.DesativarAsync(criado.Id, null);

        Assert.False((await _sut.BuscarPorIdAsync(criado.Id)).Ativo);
    }

    [Fact]
    public async Task Desativar_JaDesativado_LancaConflict()
    {
        var criado = await _sut.CriarAsync(Req());
        await _sut.DesativarAsync(criado.Id, null);

        var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.DesativarAsync(criado.Id, null));
        Assert.Equal("CLIENTE_JA_DESATIVADO", ex.Code);
    }

    [Fact]
    public async Task Desativar_MotivoInvalido_LancaArgumentException()
    {
        var criado = await _sut.CriarAsync(Req());

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.DesativarAsync(criado.Id, "qualquer-coisa"));
        Assert.True((await _sut.BuscarPorIdAsync(criado.Id)).Ativo);
    }

    [Fact]
    public async Task Desativar_GravaUmEventoNoOutboxComEnvelopeDoContrato()
    {
        var criado = await _sut.CriarAsync(Req());

        await _sut.DesativarAsync(criado.Id, "inadimplencia");

        var item = Assert.Single(_outbox.Itens);
        Assert.Equal("cliente.desativado", item.Type);
        Assert.Null(item.PublishedAt);

        using var doc = JsonDocument.Parse(item.Payload);
        var raiz = doc.RootElement;
        Assert.Equal(item.Id, raiz.GetProperty("eventId").GetGuid());
        Assert.Equal("cliente.desativado", raiz.GetProperty("eventType").GetString());
        Assert.Equal("1.0", raiz.GetProperty("eventVersion").GetString());
        Assert.Equal("cadastros-service", raiz.GetProperty("producer").GetString());
        Assert.Matches(new Regex("^00-[0-9a-f]{32}-[0-9a-f]{16}-[0-9a-f]{2}$"), raiz.GetProperty("correlationId").GetString()!);
        Assert.True(raiz.GetProperty("occurredAt").GetDateTime() > DateTime.UtcNow.AddMinutes(-1));

        var data = raiz.GetProperty("data");
        Assert.Equal(criado.Id, data.GetProperty("clienteId").GetGuid());
        Assert.Equal("inadimplencia", data.GetProperty("motivo").GetString());
        Assert.True(data.TryGetProperty("desativadoEm", out _));
    }

    [Fact]
    public async Task Desativar_SemMotivo_EventoUsaOutro()
    {
        var criado = await _sut.CriarAsync(Req());

        await _sut.DesativarAsync(criado.Id, null);

        using var doc = JsonDocument.Parse(Assert.Single(_outbox.Itens).Payload);
        Assert.Equal("outro", doc.RootElement.GetProperty("data").GetProperty("motivo").GetString());
    }

    [Fact]
    public async Task Desativar_ComSucesso_SalvaClienteEEventoNaMesmaChamada()
    {
        var criado = await _sut.CriarAsync(Req());
        var salvamentosAntes = _uow.Saves;

        await _sut.DesativarAsync(criado.Id, null);

        // um único SaveChanges persiste a mudança do cliente e o evento (mesma transação)
        Assert.Equal(salvamentosAntes + 1, _uow.Saves);
        Assert.Single(_outbox.Itens);
    }

    [Fact]
    public async Task Desativar_JaDesativado_NaoGeraSegundoEvento()
    {
        var criado = await _sut.CriarAsync(Req());
        await _sut.DesativarAsync(criado.Id, null);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.DesativarAsync(criado.Id, null));

        Assert.Single(_outbox.Itens);
    }

    [Fact]
    public async Task Desativar_ClienteInexistente_NaoGeraEvento()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DesativarAsync(Guid.NewGuid(), null));

        Assert.Empty(_outbox.Itens);
    }

    [Fact]
    public async Task Desativar_MotivoInvalido_NaoGeraEvento()
    {
        var criado = await _sut.CriarAsync(Req());

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.DesativarAsync(criado.Id, "xyz"));

        Assert.Empty(_outbox.Itens);
    }
}
