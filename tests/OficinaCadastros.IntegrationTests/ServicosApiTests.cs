using System.Net;
using System.Net.Http.Json;
using OficinaCadastros.API.Middleware;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class ServicosApiTests
{
    private readonly HttpClient _client;

    public ServicosApiTests(CadastrosApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Criar_Retorna201_EPodeSerBuscado()
    {
        var resp = await _client.PostAsJsonAsync("/servicos", TestData.NovoServico());

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var criado = await resp.Content.ReadFromJsonAsync<ServicoDto>();
        Assert.Equal(80m, criado!.PrecoUnitario);
        Assert.Equal(0.5m, criado.TempoEstimadoHoras);

        var lido = await _client.GetFromJsonAsync<ServicoDto>($"/servicos/{criado.Id}");
        Assert.Equal("Troca de óleo", lido!.Nome);
    }

    [Fact]
    public async Task Criar_PrecoNegativo_Retorna400()
    {
        var req = TestData.NovoServico();
        req.PrecoUnitario = -10m;

        var resp = await _client.PostAsJsonAsync("/servicos", req);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task BuscarPorId_Inexistente_Retorna404()
    {
        var resp = await _client.GetAsync($"/servicos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("SERVICO_NAO_ENCONTRADO", erro!.Code);
    }

    [Fact]
    public async Task Atualizar_AlteraTodosOsCampos()
    {
        var servico = await TestData.CriarServicoAsync(_client);

        var resp = await _client.PutAsJsonAsync($"/servicos/{servico.Id}", new AtualizarServicoRequest
        {
            Nome = "Alinhamento",
            PrecoUnitario = 150m,
            TempoEstimadoHoras = 1.5m
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var lido = await _client.GetFromJsonAsync<ServicoDto>($"/servicos/{servico.Id}");
        Assert.Equal("Alinhamento", lido!.Nome);
        Assert.Equal(150m, lido.PrecoUnitario);
        Assert.Equal(1.5m, lido.TempoEstimadoHoras);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Retorna404()
    {
        var resp = await _client.PutAsJsonAsync($"/servicos/{Guid.NewGuid()}", new AtualizarServicoRequest
        {
            Nome = "X",
            PrecoUnitario = 1,
            TempoEstimadoHoras = 1
        });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
