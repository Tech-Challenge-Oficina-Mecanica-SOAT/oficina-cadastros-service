using System.Net;
using OficinaCadastros.Application.Dtos;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.BDD.Support;

/// <summary>Estado de um cenário, compartilhado entre os passos (o Reqnroll cria um por cenário).</summary>
public class ContextoBdd
{
    public HttpClient Http { get; } = AmbienteBdd.Factory.CreateClient();

    public ClienteDto? Cliente { get; set; }
    public HttpStatusCode? StatusCadastroCliente { get; set; }

    public VeiculoDto? Veiculo { get; set; }
    public HttpStatusCode? StatusCadastroVeiculo { get; set; }
    public VeiculoDto? VeiculoEncontrado { get; set; }

    /// <summary>Última resposta de uma requisição que o cenário espera ver recusada.</summary>
    public HttpResponseMessage? UltimaResposta { get; set; }

    public OutboxEvent? EventoPublicado { get; set; }
}
