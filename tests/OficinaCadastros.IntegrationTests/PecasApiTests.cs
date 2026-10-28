using System.Net;
using System.Net.Http.Json;
using OficinaCadastros.API.Middleware;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class PecasApiTests
{
    private readonly HttpClient _client;

    public PecasApiTests(CadastrosApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Criar_NormalizaCodigo_ERetornaPrecoComoNumero()
    {
        var codigo = TestData.CodigoPeca();

        var resp = await _client.PostAsJsonAsync("/pecas", TestData.NovaPeca(codigo.ToLowerInvariant()));

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var criada = await resp.Content.ReadFromJsonAsync<PecaDto>();
        Assert.Equal(codigo.ToUpperInvariant(), criada!.Codigo);
        Assert.Equal(35.90m, criada.PrecoUnitario);
        Assert.Equal(10, criada.Estoque);

        var lida = await _client.GetFromJsonAsync<PecaDto>($"/pecas/{criada.Id}");
        Assert.Equal(35.90m, lida!.PrecoUnitario);
    }

    [Fact]
    public async Task Criar_CodigoDuplicadoEmOutraCaixa_Retorna409()
    {
        var codigo = TestData.CodigoPeca();
        await TestData.CriarPecaAsync(_client, codigo);

        var resp = await _client.PostAsJsonAsync("/pecas", TestData.NovaPeca(codigo.ToLowerInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("CODIGO_JA_CADASTRADO", erro!.Code);
    }

    [Fact]
    public async Task Criar_EstoqueNegativo_Retorna400()
    {
        var req = TestData.NovaPeca();
        req.Estoque = -1;

        var resp = await _client.PostAsJsonAsync("/pecas", req);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Criar_ComEstoqueZero_Retorna201()
    {
        var req = TestData.NovaPeca();
        req.Estoque = 0;

        var resp = await _client.PostAsJsonAsync("/pecas", req);

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
    }

    [Fact]
    public async Task Listar_IncluiPecaCriada_EOrdenaPorNome()
    {
        var a = TestData.NovaPeca();
        a.Nome = "AAA primeira " + Guid.NewGuid().ToString("N")[..6];
        var b = TestData.NovaPeca();
        b.Nome = "ZZZ ultima " + Guid.NewGuid().ToString("N")[..6];
        await _client.PostAsJsonAsync("/pecas", b);
        await _client.PostAsJsonAsync("/pecas", a);

        var lista = await _client.GetFromJsonAsync<List<PecaDto>>("/pecas");

        Assert.NotNull(lista);
        var nomes = lista!.Select(p => p.Nome).ToList();
        Assert.Contains(a.Nome, nomes);
        Assert.Contains(b.Nome, nomes);
        Assert.True(nomes.IndexOf(a.Nome) < nomes.IndexOf(b.Nome));
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_Retorna404()
    {
        var resp = await _client.GetAsync($"/pecas/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PECA_NAO_ENCONTRADA", erro!.Code);
    }

    [Fact]
    public async Task Atualizar_AlteraDados_MantemCodigo()
    {
        var peca = await TestData.CriarPecaAsync(_client);

        var resp = await _client.PutAsJsonAsync($"/pecas/{peca.Id}", new AtualizarPecaRequest
        {
            Nome = "Filtro premium",
            PrecoUnitario = 49.90m,
            Estoque = 3
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var lida = await _client.GetFromJsonAsync<PecaDto>($"/pecas/{peca.Id}");
        Assert.Equal("Filtro premium", lida!.Nome);
        Assert.Equal(49.90m, lida.PrecoUnitario);
        Assert.Equal(3, lida.Estoque);
        Assert.Equal(peca.Codigo, lida.Codigo);
    }

    [Fact]
    public async Task Atualizar_EstoqueNegativo_Retorna400()
    {
        var peca = await TestData.CriarPecaAsync(_client);

        var resp = await _client.PutAsJsonAsync($"/pecas/{peca.Id}", new AtualizarPecaRequest
        {
            Nome = "X",
            PrecoUnitario = 1,
            Estoque = -5
        });

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Retorna404()
    {
        var resp = await _client.PutAsJsonAsync($"/pecas/{Guid.NewGuid()}", new AtualizarPecaRequest
        {
            Nome = "X",
            PrecoUnitario = 1,
            Estoque = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
