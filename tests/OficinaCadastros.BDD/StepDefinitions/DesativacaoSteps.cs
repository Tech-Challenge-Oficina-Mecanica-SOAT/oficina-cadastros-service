using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.BDD.Support;
using OficinaCadastros.Domain.Entities;
using OficinaCadastros.Infrastructure.Persistence;
using Reqnroll;

namespace OficinaCadastros.BDD.StepDefinitions;

[Binding]
public class DesativacaoSteps
{
    private readonly ContextoBdd _ctx;

    public DesativacaoSteps(ContextoBdd ctx) => _ctx = ctx;

    [Given("que existe um cliente ativo com CPF {string}")]
    public async Task DadoQueExisteUmClienteAtivo(string cpf)
    {
        var resp = await _ctx.Http.PostAsJsonAsync("/clientes", new CriarClienteRequest
        {
            Nome = "Cliente de Teste",
            Cpf = cpf,
            Email = "teste@email.com"
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        _ctx.Cliente = await resp.Content.ReadFromJsonAsync<ClienteDto>();
    }

    [Given("o cliente já foi desativado")]
    public async Task DadoQueOClienteJaFoiDesativado()
    {
        var resp = await _ctx.Http.PostAsync($"/clientes/{_ctx.Cliente!.Id}/desativar", content: null);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

        // espera o primeiro evento sair, para que a contagem do cenário não dependa de timing
        var primeiro = await Espera.AteAsync(() => Task.FromResult(EventoPublicadoPara(_ctx.Cliente.Id)));
        Assert.NotNull(primeiro);
    }

    [When("desativo o cliente informando o motivo {string}")]
    public async Task QuandoDesativoOCliente(string motivo)
    {
        var resp = await _ctx.Http.PostAsJsonAsync($"/clientes/{_ctx.Cliente!.Id}/desativar",
            new DesativarClienteRequest { Motivo = motivo });

        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);
    }

    [When("tento desativar o cliente novamente")]
    public async Task QuandoTentoDesativarNovamente()
    {
        _ctx.UltimaResposta = await _ctx.Http.PostAsync($"/clientes/{_ctx.Cliente!.Id}/desativar", content: null);
    }

    [Then("o cliente passa a constar como inativo")]
    public async Task EntaoOClientePassaAConstarComoInativo()
    {
        var cliente = await _ctx.Http.GetFromJsonAsync<ClienteDto>($"/clientes/{_ctx.Cliente!.Id}");

        Assert.NotNull(cliente);
        Assert.False(cliente!.Ativo);
    }

    [Then("o evento {string} é publicado com o motivo {string}")]
    public async Task EntaoOEventoEPublicadoComOMotivo(string tipo, string motivo)
    {
        _ctx.EventoPublicado = await Espera.AteAsync(() => Task.FromResult(EventoPublicadoPara(_ctx.Cliente!.Id)));

        Assert.NotNull(_ctx.EventoPublicado);
        Assert.Equal(tipo, _ctx.EventoPublicado!.Type);

        using var doc = JsonDocument.Parse(_ctx.EventoPublicado.Payload);
        Assert.Equal(motivo, doc.RootElement.GetProperty("data").GetProperty("motivo").GetString());
    }

    [Then("o evento publicado identifica o cliente desativado")]
    public void EntaoOEventoIdentificaOCliente()
    {
        using var doc = JsonDocument.Parse(_ctx.EventoPublicado!.Payload);
        var raiz = doc.RootElement;

        Assert.Equal(_ctx.Cliente!.Id, raiz.GetProperty("data").GetProperty("clienteId").GetGuid());
        Assert.Equal("cadastros-service", raiz.GetProperty("producer").GetString());
        Assert.Equal("1.0", raiz.GetProperty("eventVersion").GetString());
        Assert.Equal(_ctx.EventoPublicado.Id, raiz.GetProperty("eventId").GetGuid());
    }

    [Then("o evento fica registrado como publicado no outbox")]
    public async Task EntaoOEventoFicaRegistradoComoPublicado()
    {
        var salvo = await Espera.AteAsync(async () =>
        {
            using var scope = AmbienteBdd.Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CadastrosDbContext>();
            return await db.OutboxEvents.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == _ctx.EventoPublicado!.Id && e.PublishedAt != null);
        });

        Assert.NotNull(salvo);
    }

    [Then("nenhum novo evento é publicado para esse cliente")]
    public void EntaoNenhumNovoEventoEPublicado()
    {
        var id = _ctx.Cliente!.Id.ToString();
        var total = AmbienteBdd.Factory.Publisher.Published.Count(e => e.Payload.Contains(id));

        Assert.Equal(1, total);
    }

    private static OutboxEvent? EventoPublicadoPara(Guid clienteId) =>
        AmbienteBdd.Factory.Publisher.Published.FirstOrDefault(e => e.Payload.Contains(clienteId.ToString()));
}
