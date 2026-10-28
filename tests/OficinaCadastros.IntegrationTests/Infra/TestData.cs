using System.Net.Http.Json;
using OficinaCadastros.Application.Dtos;

namespace OficinaCadastros.IntegrationTests.Infra;

public static class TestData
{
    // Valores únicos por chamada: o banco é compartilhado entre os testes.
    public static string Cpf() => Random.Shared.NextInt64(10_000_000_000, 100_000_000_000).ToString();
    public static string Placa() => "T" + Random.Shared.Next(0, 1_000_000).ToString("D6");
    public static string CodigoPeca() => "P-" + Guid.NewGuid().ToString("N")[..10];

    public static CriarClienteRequest NovoCliente(string? cpf = null) => new()
    {
        Nome = "Maria da Silva",
        Cpf = cpf ?? Cpf(),
        Email = "maria@email.com",
        Telefone = "62999990000"
    };

    public static CriarVeiculoRequest NovoVeiculo(Guid clienteId, string? placa = null) => new()
    {
        Placa = placa ?? Placa(),
        Modelo = "Onix",
        Marca = "Chevrolet",
        Ano = 2021,
        ClienteId = clienteId
    };

    public static CriarPecaRequest NovaPeca(string? codigo = null) => new()
    {
        Codigo = codigo ?? CodigoPeca(),
        Nome = "Filtro de óleo",
        PrecoUnitario = 35.90m,
        Estoque = 10
    };

    public static CriarServicoRequest NovoServico() => new()
    {
        Nome = "Troca de óleo",
        PrecoUnitario = 80m,
        TempoEstimadoHoras = 0.5m
    };

    public static async Task<ClienteDto> CriarClienteAsync(HttpClient http, string? cpf = null)
    {
        var resp = await http.PostAsJsonAsync("/clientes", NovoCliente(cpf));
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<ClienteDto>())!;
    }

    public static async Task<VeiculoDto> CriarVeiculoAsync(HttpClient http, Guid clienteId, string? placa = null)
    {
        var resp = await http.PostAsJsonAsync("/veiculos", NovoVeiculo(clienteId, placa));
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<VeiculoDto>())!;
    }

    public static async Task<PecaDto> CriarPecaAsync(HttpClient http, string? codigo = null)
    {
        var resp = await http.PostAsJsonAsync("/pecas", NovaPeca(codigo));
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<PecaDto>())!;
    }

    public static async Task<ServicoDto> CriarServicoAsync(HttpClient http)
    {
        var resp = await http.PostAsJsonAsync("/servicos", NovoServico());
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<ServicoDto>())!;
    }

    /// <summary>Repete a busca até devolver algo ou estourar o tempo (o publicador do outbox é assíncrono).</summary>
    public static async Task<T?> EsperarAsync<T>(Func<Task<T?>> busca, int timeoutSegundos = 15) where T : class
    {
        var limite = DateTime.UtcNow.AddSeconds(timeoutSegundos);
        while (DateTime.UtcNow < limite)
        {
            var resultado = await busca();
            if (resultado is not null) return resultado;
            await Task.Delay(250);
        }
        return null;
    }
}
