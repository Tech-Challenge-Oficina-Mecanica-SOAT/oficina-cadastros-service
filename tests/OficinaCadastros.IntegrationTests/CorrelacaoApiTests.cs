using System.Net;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class CorrelacaoApiTests
{
    private const string Header = "X-Correlation-Id";

    private readonly HttpClient _client;

    public CorrelacaoApiTests(CadastrosApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Resposta_TrazOHeaderDeCorrelacao()
    {
        var resp = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.True(resp.Headers.TryGetValues(Header, out var valores));
        Assert.Matches("^[0-9a-f]{32}$", valores!.Single());
    }

    [Fact]
    public async Task ContinuaATraceRecebidaNoTraceparent()
    {
        var traceId = Guid.NewGuid().ToString("N");
        var req = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        req.Headers.Add("traceparent", $"00-{traceId}-00f067aa0ba902b7-01");

        var resp = await _client.SendAsync(req);

        Assert.True(resp.Headers.TryGetValues(Header, out var valores));
        Assert.Equal(traceId, valores!.Single());
    }

    [Fact]
    public async Task RespostaDeErroTambemTrazOHeader()
    {
        var resp = await _client.GetAsync($"/clientes/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        Assert.True(resp.Headers.Contains(Header));
    }

    [Fact]
    public async Task CadaRequisicaoSemTraceparentRecebeUmIdDiferente()
    {
        var a = await _client.GetAsync("/health/live");
        var b = await _client.GetAsync("/health/live");

        Assert.NotEqual(a.Headers.GetValues(Header).Single(), b.Headers.GetValues(Header).Single());
    }
}
