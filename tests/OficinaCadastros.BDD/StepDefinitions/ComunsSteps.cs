using System.Net;
using System.Net.Http.Json;
using OficinaCadastros.API.Middleware;
using OficinaCadastros.BDD.Support;
using Reqnroll;

namespace OficinaCadastros.BDD.StepDefinitions;

[Binding]
public class ComunsSteps
{
    private readonly ContextoBdd _ctx;

    public ComunsSteps(ContextoBdd ctx) => _ctx = ctx;

    [Given("que a API de cadastros está disponível")]
    public async Task DadoQueAApiEstaDisponivel()
    {
        var resp = await _ctx.Http.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Then("a requisição é recusada por conflito com o código {string}")]
    public async Task EntaoARequisicaoERecusadaPorConflito(string codigo)
    {
        Assert.NotNull(_ctx.UltimaResposta);
        Assert.Equal(HttpStatusCode.Conflict, _ctx.UltimaResposta!.StatusCode);

        var erro = await _ctx.UltimaResposta.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal(codigo, erro!.Code);
    }
}
