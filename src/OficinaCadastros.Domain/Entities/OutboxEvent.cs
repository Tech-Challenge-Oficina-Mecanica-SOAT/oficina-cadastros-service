namespace OficinaCadastros.Domain.Entities;

/// <summary>
/// Evento de integração gravado na mesma transação da mudança de negócio (Outbox Pattern)
/// e publicado depois, de forma assíncrona, no SNS.
/// </summary>
public class OutboxEvent
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty;
    /// <summary>Envelope completo (JSON) que será publicado como corpo da mensagem.</summary>
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public int Attempts { get; private set; }
    public string? LastError { get; private set; }

    private OutboxEvent() { }

    public OutboxEvent(Guid id, string type, string payload, DateTime occurredAt)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id do evento é obrigatório.", nameof(id));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Tipo do evento é obrigatório.", nameof(type));
        if (string.IsNullOrWhiteSpace(payload)) throw new ArgumentException("Payload do evento é obrigatório.", nameof(payload));

        Id = id;
        Type = type;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public void MarcarPublicado(DateTime quando)
    {
        PublishedAt = quando;
        LastError = null;
    }

    public void RegistrarFalha(string erro)
    {
        Attempts++;
        LastError = erro.Length > 1000 ? erro[..1000] : erro;
    }
}
