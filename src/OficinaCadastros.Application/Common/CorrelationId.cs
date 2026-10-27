using System.Diagnostics;

namespace OficinaCadastros.Application.Common;

public static class CorrelationId
{
    /// <summary>
    /// W3C traceparent da requisição atual; se não houver Activity (job, teste), gera um novo.
    /// Formato: 00-{traceId 32 hex}-{spanId 16 hex}-{flags}
    /// </summary>
    public static string Atual()
    {
        var id = Activity.Current?.Id;
        if (id is { Length: 55 } && id.StartsWith("00-", StringComparison.Ordinal))
            return id;

        return $"00-{ActivityTraceId.CreateRandom()}-{ActivitySpanId.CreateRandom()}-01";
    }
}
