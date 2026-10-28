using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OficinaCadastros.Domain.Entities;
using OficinaCadastros.Infrastructure.Persistence;
using OficinaCadastros.IntegrationTests.Infra;

namespace OficinaCadastros.IntegrationTests;

[Collection("api")]
public class OutboxApiTests
{
    private readonly CadastrosApiFactory _factory;
    private readonly HttpClient _client;

    public OutboxApiTests(CadastrosApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Outbox_ComFalhasTransitorias_TentaDeNovoAteConseguirPublicar()
    {
        var cliente = await TestData.CriarClienteAsync(_client);
        _factory.Publisher.FalharPara(cliente.Id, vezes: 2);

        var resp = await _client.PostAsync($"/clientes/{cliente.Id}/desativar", content: null);
        Assert.Equal(HttpStatusCode.NoContent, resp.StatusCode);

        // o publicador consulta a cada 1 s: duas falhas + uma publicação levam alguns segundos
        var publicado = await TestData.EsperarAsync(() => BuscarEventoPublicadoAsync(cliente.Id), timeoutSegundos: 30);

        Assert.NotNull(publicado);
        Assert.Equal(2, publicado!.Attempts);
        Assert.Null(publicado.LastError);
        Assert.Contains(_factory.Publisher.Published, e => e.Id == publicado.Id);
    }

    private async Task<OutboxEvent?> BuscarEventoPublicadoAsync(Guid clienteId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CadastrosDbContext>();

        // Payload é jsonb: o filtro por conteúdo é feito em memória
        var publicados = await db.OutboxEvents.AsNoTracking()
            .Where(e => e.PublishedAt != null)
            .ToListAsync();

        return publicados.FirstOrDefault(e => e.Payload.Contains(clienteId.ToString()));
    }
}
