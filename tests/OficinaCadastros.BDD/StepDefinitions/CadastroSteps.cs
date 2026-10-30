using System.Net;
using System.Net.Http.Json;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.BDD.Support;
using Reqnroll;

namespace OficinaCadastros.BDD.StepDefinitions;

[Binding]
public class CadastroSteps
{
    private readonly ContextoBdd _ctx;

    public CadastroSteps(ContextoBdd ctx) => _ctx = ctx;

    [Given("que já existe um cliente cadastrado com o CPF {string}")]
    public async Task DadoQueJaExisteUmClienteComOCpf(string cpf)
    {
        var resp = await _ctx.Http.PostAsJsonAsync("/clientes", NovoCliente("Cliente Existente", cpf, "existente@email.com"));
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        _ctx.Cliente = await resp.Content.ReadFromJsonAsync<ClienteDto>();
    }

    [When("cadastro o cliente {string} com CPF {string} e e-mail {string}")]
    public async Task QuandoCadastroOCliente(string nome, string cpf, string email)
    {
        var resp = await _ctx.Http.PostAsJsonAsync("/clientes", NovoCliente(nome, cpf, email));
        _ctx.UltimaResposta = resp;
        _ctx.StatusCadastroCliente = resp.StatusCode;

        if (resp.StatusCode == HttpStatusCode.Created)
            _ctx.Cliente = await resp.Content.ReadFromJsonAsync<ClienteDto>();
    }

    [When("cadastro o veículo de placa {string} modelo {string} marca {string} e ano {int} para esse cliente")]
    public async Task QuandoCadastroOVeiculo(string placa, string modelo, string marca, int ano)
    {
        Assert.NotNull(_ctx.Cliente);

        var resp = await _ctx.Http.PostAsJsonAsync("/veiculos", new CriarVeiculoRequest
        {
            Placa = placa,
            Modelo = modelo,
            Marca = marca,
            Ano = ano,
            ClienteId = _ctx.Cliente!.Id
        });
        _ctx.StatusCadastroVeiculo = resp.StatusCode;

        if (resp.StatusCode == HttpStatusCode.Created)
            _ctx.Veiculo = await resp.Content.ReadFromJsonAsync<VeiculoDto>();
    }

    [Then("o cliente é criado e fica ativo")]
    public void EntaoOClienteECriadoEFicaAtivo()
    {
        Assert.Equal(HttpStatusCode.Created, _ctx.StatusCadastroCliente);
        Assert.NotNull(_ctx.Cliente);
        Assert.True(_ctx.Cliente!.Ativo);
    }

    [Then("o veículo é criado com a placa {string}")]
    public void EntaoOVeiculoECriadoComAPlaca(string placa)
    {
        Assert.Equal(HttpStatusCode.Created, _ctx.StatusCadastroVeiculo);
        Assert.NotNull(_ctx.Veiculo);
        Assert.Equal(placa, _ctx.Veiculo!.Placa);
    }

    [Then("consigo encontrar o cliente pelo CPF {string}")]
    public async Task EntaoConsigoEncontrarOClientePeloCpf(string cpf)
    {
        var encontrado = await _ctx.Http.GetFromJsonAsync<ClienteDto>($"/clientes/cpf/{cpf}");

        Assert.NotNull(encontrado);
        Assert.Equal(_ctx.Cliente!.Id, encontrado!.Id);
    }

    [Then("consigo encontrar o veículo pela placa {string}")]
    public async Task EntaoConsigoEncontrarOVeiculoPelaPlaca(string placa)
    {
        _ctx.VeiculoEncontrado = await _ctx.Http.GetFromJsonAsync<VeiculoDto>($"/veiculos/placa/{placa}");

        Assert.NotNull(_ctx.VeiculoEncontrado);
        Assert.Equal(_ctx.Veiculo!.Id, _ctx.VeiculoEncontrado!.Id);
    }

    [Then("o veículo encontrado pertence a esse cliente")]
    public void EntaoOVeiculoEncontradoPertenceAEsseCliente()
    {
        Assert.Equal(_ctx.Cliente!.Id, _ctx.VeiculoEncontrado!.ClienteId);
    }

    private static CriarClienteRequest NovoCliente(string nome, string cpf, string email) => new()
    {
        Nome = nome,
        Cpf = cpf,
        Email = email
    };
}
