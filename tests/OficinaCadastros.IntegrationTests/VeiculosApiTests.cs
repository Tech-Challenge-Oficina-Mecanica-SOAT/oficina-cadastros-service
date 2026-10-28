using System.Net;
using System.Net.Http.Json;
using OficinaCadastros.API.Middleware;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class VeiculosApiTests
{
    private readonly HttpClient _client;

    public VeiculosApiTests(CadastrosApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Criar_NormalizaPlaca_EPodeSerBuscadoPorPlacaEmQualquerCaixa()
    {
        var cliente = await TestData.CriarClienteAsync(_client);
        var placa = TestData.Placa();

        var resp = await _client.PostAsJsonAsync("/veiculos", TestData.NovoVeiculo(cliente.Id, placa.ToLowerInvariant()));

        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var criado = await resp.Content.ReadFromJsonAsync<VeiculoDto>();
        Assert.Equal(placa.ToUpperInvariant(), criado!.Placa);
        Assert.Equal(cliente.Id, criado.ClienteId);

        var porPlaca = await _client.GetFromJsonAsync<VeiculoDto>($"/veiculos/placa/{placa.ToLowerInvariant()}");
        Assert.Equal(criado.Id, porPlaca!.Id);

        var porId = await _client.GetFromJsonAsync<VeiculoDto>($"/veiculos/{criado.Id}");
        Assert.Equal(criado.Placa, porId!.Placa);
    }

    [Fact]
    public async Task Criar_PlacaDuplicadaEmOutraCaixa_Retorna409()
    {
        var cliente = await TestData.CriarClienteAsync(_client);
        var placa = TestData.Placa();
        await TestData.CriarVeiculoAsync(_client, cliente.Id, placa);

        var resp = await _client.PostAsJsonAsync("/veiculos", TestData.NovoVeiculo(cliente.Id, placa.ToLowerInvariant()));

        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("PLACA_JA_CADASTRADA", erro!.Code);
    }

    [Fact]
    public async Task Criar_ClienteInexistente_Retorna404()
    {
        var resp = await _client.PostAsJsonAsync("/veiculos", TestData.NovoVeiculo(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        var erro = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("CLIENTE_NAO_ENCONTRADO", erro!.Code);
    }

    [Fact]
    public async Task Criar_AnoInvalido_Retorna400()
    {
        var cliente = await TestData.CriarClienteAsync(_client);
        var req = TestData.NovoVeiculo(cliente.Id);
        req.Ano = 1800;

        var resp = await _client.PostAsJsonAsync("/veiculos", req);

        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task BuscarPorPlaca_Inexistente_Retorna404()
    {
        var resp = await _client.GetAsync("/veiculos/placa/ZZZ9Z99");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Atualizar_AlteraModeloMarcaEAno_MantemPlacaECliente()
    {
        var cliente = await TestData.CriarClienteAsync(_client);
        var veiculo = await TestData.CriarVeiculoAsync(_client, cliente.Id);

        var resp = await _client.PutAsJsonAsync($"/veiculos/{veiculo.Id}", new AtualizarVeiculoRequest
        {
            Modelo = "Tracker",
            Marca = "Chevrolet",
            Ano = 2023
        });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var lido = await _client.GetFromJsonAsync<VeiculoDto>($"/veiculos/{veiculo.Id}");
        Assert.Equal("Tracker", lido!.Modelo);
        Assert.Equal(2023, lido.Ano);
        Assert.Equal(veiculo.Placa, lido.Placa);
        Assert.Equal(cliente.Id, lido.ClienteId);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Retorna404()
    {
        var resp = await _client.PutAsJsonAsync($"/veiculos/{Guid.NewGuid()}", new AtualizarVeiculoRequest
        {
            Modelo = "X",
            Marca = "Y",
            Ano = 2020
        });

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
