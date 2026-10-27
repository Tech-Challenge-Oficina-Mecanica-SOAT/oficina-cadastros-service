namespace OficinaCadastros.Infrastructure.Messaging;

public class MessagingOptions
{
    /// <summary>ARN do tópico SNS cliente.desativado. Vazio = só registra no log (desenvolvimento local).</summary>
    public string? ClienteDesativadoTopicArn { get; set; }

    public string Region { get; set; } = "us-east-1";

    public int PollIntervalSeconds { get; set; } = 5;

    public int BatchSize { get; set; } = 20;

    /// <summary>Depois dessa quantidade de falhas o evento deixa de ser tentado (fica na tabela para análise).</summary>
    public int MaxAttempts { get; set; } = 10;
}
