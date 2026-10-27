using System.Text.Json;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Application.Events;

/// <summary>Monta o envelope do evento cliente.desativado (contratos/eventos/schemas.md).</summary>
public static class ClienteDesativadoEvent
{
    public const string Tipo = "cliente.desativado";
    public const string Versao = "1.0";
    public const string Producer = "cadastros-service";
    public const string MotivoPadrao = "outro";

    private static readonly JsonSerializerOptions Opcoes = new(JsonSerializerDefaults.Web);

    public static OutboxEvent Criar(Guid clienteId, string? motivo, DateTime agoraUtc, string correlationId)
    {
        var eventId = Guid.NewGuid();

        var envelope = new
        {
            eventId,
            eventType = Tipo,
            eventVersion = Versao,
            occurredAt = agoraUtc,
            correlationId,
            producer = Producer,
            data = new
            {
                clienteId,
                motivo = motivo ?? MotivoPadrao,
                desativadoEm = agoraUtc
            }
        };

        return new OutboxEvent(eventId, Tipo, JsonSerializer.Serialize(envelope, Opcoes), agoraUtc);
    }
}
