using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OficinaCadastros.API.Middleware;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Domain.Entities;
using OficinaCadastros.Infrastructure.Persistence;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class ClientesApiTests
{
    private readonly CadastrosApiFactory _factory;
    private readonly HttpClient _client;

    public ClientesApiTests(CadastrosApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Criar_Retorna201ComLocation_EPodeSerBuscadoPorIdECpf()
    {
        var cpf = TestData.Cpf();

        var resp = await _client.PostAsJsonAsync("/clientes", TestData.NovoCliente(cpf));

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var criado = await resp.Content.ReadFromJsonAsync<ClienteDto>();
        Assert.NotNull(criado);
        Assert.True(criado!.Ativo);
        Assert.EndsWith($"/clientes/{criado.Id}", resp.Headers.Location!.ToString());

        var porId = await _client.GetFromJsonAsync<ClienteDto>($"/clientes/{criado.Id}");
        Assert.Equal(cpf, porId!.Cpf);

        var porCpf = await _client.GetFromJsonAsync<ClienteDto>($"/clientes/cpf/{cpf}");
        Assert.Equal(criado.Id, porCpf!.Id);
    }

    [Fact]
    public async Task Criar_CpfDuplicado_Retorna409NoFormatoDoContrato()
    {
        var cpf = TestData.Cpf();
        await TestData.CriarClienteAsync(_client, cpf);

        var resp = await _client.PostAsJsonAsync("/clientes", TestData.NovoCliente(cpf));

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("CPF_JA_CADASTRADO", erro!.Code);
        Assert.False(string.IsNullOrWhiteSpace(erro.Message));
    }

    [Fact]
    public async Task Criar_CpfInvalido_Retorna400()
    {
        var resp = await _client.PostAsJsonAsync("/clientes", TestData.NovoCliente("123"));

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("REQUISICAO_INVALIDA", erro!.Code);
        Assert.Contains("CPF", erro.Message);
    }

    [Fact]
    public async Task Criar_SemEmail_Retorna400()
    {
        var req = TestData.NovoCliente();
        req.Email = "";

        var resp = await _client.PostAsJsonAsync("/clientes", req);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_Retorna404()
    {
        var resp = await _client.GetAsync($"/clientes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", erro!.Code);
    }

    [Fact]
    public async Task BuscarPorCpf_FormatoInvalido_Retorna400()
    {
        var resp = await _client.GetAsync("/clientes/cpf/abc");

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task BuscarPorCpf_Inexistente_Retorna404()
    {
        var resp = await _client.GetAsync($"/clientes/cpf/{TestData.Cpf()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Atualizar_PersisteAsAlteracoes()
    {
        var cliente = await TestData.CriarClienteAsync(_client);

        var resp = await _client.PutAsJsonAsync($"/clientes/{cliente.Id}", new AtualizarClienteRequest
        {
            Nome = "Maria Souza",
            Email = "maria.souza@email.com",
            Telefone = null
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var lido = await _client.GetFromJsonAsync<ClienteDto>($"/clientes/{cliente.Id}");
        Assert.Equal("Maria Souza", lido!.Nome);
        Assert.Null(lido.Telefone);
        Assert.Equal(cliente.Cpf, lido.Cpf);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Retorna404()
    {
        var resp = await _client.PutAsJsonAsync($"/clientes/{Guid.NewGuid()}", new AtualizarClienteRequest
        {
            Nome = "X",
            Email = "x@x.com"
        });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_Retorna204_ClienteFicaInativo_ESegundaChamadaDa409()
    {
        var cliente = await TestData.CriarClienteAsync(_client);

        var primeira = await _client.PostAsJsonAsync($"/clientes/{cliente.Id}/desativar",
            new DesativarClienteRequest { Motivo = "solicitacao_cliente" });
        Assert.Equal(HttpStatusCode.NoContent, primeira.StatusCode);

        var lido = await _client.GetFromJsonAsync<ClienteDto>($"/clientes/{cliente.Id}");
        Assert.False(lido!.Ativo);

        var segunda = await _client.PostAsJsonAsync($"/clientes/{cliente.Id}/desativar",
            new DesativarClienteRequest());
        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }

    [Fact]
    public async Task Desativar_SemCorpoNaRequisicao_Retorna204()
    {
        var cliente = await TestData.CriarClienteAsync(_client);

        var resp = await _client.PostAsync($"/clientes/{cliente.Id}/desativar", content: null);

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_MotivoInvalido_Retorna400_ENaoDesativa()
    {
        var cliente = await TestData.CriarClienteAsync(_client);

        var resp = await _client.PostAsJsonAsync($"/clientes/{cliente.Id}/desativar",
            new DesativarClienteRequest { Motivo = "qualquer-coisa" });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var lido = await _client.GetFromJsonAsync<ClienteDto>($"/clientes/{cliente.Id}");
        Assert.True(lido!.Ativo);
    }

    [Fact]
    public async Task Desativar_InexistenteRetorna404()
    {
        var resp = await _client.PostAsync($"/clientes/{Guid.NewGuid()}/desativar", content: null);

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Desativar_GravaEventoNoOutbox_EOPublicadorOEnviaEMarcaComoPublicado()
    {
        var cliente = await TestData.CriarClienteAsync(_client);

        var resp = await _client.PostAsJsonAsync($"/clientes/{cliente.Id}/desativar",
            new DesativarClienteRequest { Motivo = "inadimplencia" });
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

        // o OutboxPublisherService roda em segundo plano: espera o evento chegar ao publicador falso
        var publicado = await TestData.EsperarAsync(() =>
            Task.FromResult(_factory.Publisher.Published.FirstOrDefault(e => e.Payload.Contains(cliente.Id.ToString()))));
        Assert.NotNull(publicado);
        Assert.Equal("cliente.desativado", publicado!.Type);

        using var doc = JsonDocument.Parse(publicado.Payload);
        var raiz = doc.RootElement;
        Assert.Equal("cadastros-service", raiz.GetProperty("producer").GetString());
        Assert.Equal(cliente.Id, raiz.GetProperty("data").GetProperty("clienteId").GetGuid());
        Assert.Equal("inadimplencia", raiz.GetProperty("data").GetProperty("motivo").GetString());

        // depois de publicar, a linha do banco é marcada com PublishedAt
        var salvo = await TestData.EsperarAsync(async () =>
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CadastrosDbContext>();
            return await db.OutboxEvents.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == publicado.Id && e.PublishedAt != null);
        });
        Assert.NotNull(salvo);
        Assert.Equal(0, salvo!.Attempts);
    }

    [Fact]
    public async Task Desativar_Falho_NaoGeraEventoNoOutbox()
    {
        var cliente = await TestData.CriarClienteAsync(_client);
        await _client.PostAsync($"/clientes/{cliente.Id}/desativar", content: null);
        await _client.PostAsync($"/clientes/{cliente.Id}/desativar", content: null); // 409

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CadastrosDbContext>();
        var eventos = await db.OutboxEvents.AsNoTracking().ToListAsync();
        var doCliente = eventos.Count(e => e.Payload.Contains(cliente.Id.ToString()));

        Assert.Equal(1, doCliente);
    }
}
