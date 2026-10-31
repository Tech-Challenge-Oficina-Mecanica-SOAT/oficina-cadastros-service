using System.Diagnostics;

namespace OficinaCadastros.API.Middleware;

/// <summary>
/// Garante um correlation ID em toda requisição:
///  - é o trace id W3C da requisição (se o chamador mandou o header traceparent, a trace continua;
///    se não, o ASP.NET Core cria uma nova);
///  - entra como escopo de log (campo "correlationId" nos logs JSON);
///  - volta na resposta no header X-Correlation-Id.
/// O correlationId dos eventos do outbox é o traceparent completo, que contém esse mesmo trace id.
/// </summary>
public class CorrelationMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        // OnStarting roda no momento de enviar a resposta, então vale também para respostas de erro
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["correlationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
