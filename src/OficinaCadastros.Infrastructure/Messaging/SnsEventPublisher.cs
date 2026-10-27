using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using OficinaCadastros.Application.Abstractions;
using OficinaCadastros.Application.Events;
using OficinaCadastros.Domain.Entities;

namespace OficinaCadastros.Infrastructure.Messaging;

public class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly MessagingOptions _options;
    private readonly ILogger<SnsEventPublisher> _logger;

    public SnsEventPublisher(IAmazonSimpleNotificationService sns, MessagingOptions options, ILogger<SnsEventPublisher> logger)
    {
        _sns = sns;
        _options = options;
        _logger = logger;
    }

    public async Task PublishAsync(OutboxEvent evento, CancellationToken ct)
    {
        var topicArn = evento.Type switch
        {
            ClienteDesativadoEvent.Tipo => _options.ClienteDesativadoTopicArn,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(topicArn))
            throw new InvalidOperationException($"Nenhum tópico configurado para o evento '{evento.Type}'.");

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = evento.Payload,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["eventType"] = new MessageAttributeValue { DataType = "String", StringValue = evento.Type }
            }
        };

        var response = await _sns.PublishAsync(request, ct);
        _logger.LogInformation("Evento {Tipo} {EventId} publicado no SNS (MessageId {MessageId})",
            evento.Type, evento.Id, response.MessageId);
    }
}
