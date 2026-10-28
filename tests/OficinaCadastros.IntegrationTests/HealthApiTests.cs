using System.Net;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class HealthApiTests
{
    private readonly HttpClient _client;

    public HealthApiTests(CadastrosApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Live_Retorna200()
    {
        var resp = await _client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
    }

    [Fact]
    public async Task Ready_ComBancoNoAr_Retorna200()
    {
        var resp = await _client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        Assert.Equal("Healthy", await resp.Content.ReadAsStringAsync());
    }
}
