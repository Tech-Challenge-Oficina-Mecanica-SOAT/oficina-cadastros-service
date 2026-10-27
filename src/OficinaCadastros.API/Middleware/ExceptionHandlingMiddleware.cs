using Microsoft.EntityFrameworkCore;
using Npgsql;
using OficinaCadastros.Application.Common;

namespace OficinaCadastros.API.Middleware;

/// <summary>Formato de erro do contrato OpenAPI (schema Error).</summary>
public record ErrorResponse(string Code, string Message);

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var (status, error) = Mapear(ex);
            if (status >= 500) _logger.LogError(ex, "Erro não tratado");

            context.Response.Clear();
            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(error);
        }
    }

    private static (int Status, ErrorResponse Error) Mapear(Exception ex) => ex switch
    {
        NotFoundException nf => (StatusCodes.Status404NotFound, new ErrorResponse(nf.Code, nf.Message)),
        ConflictException c => (StatusCodes.Status409Conflict, new ErrorResponse(c.Code, c.Message)),
        ArgumentException a => (StatusCodes.Status400BadRequest, new ErrorResponse("REQUISICAO_INVALIDA", LimparMensagem(a.Message))),
        DbUpdateException { InnerException: PostgresException { SqlState: "23505" } }
            => (StatusCodes.Status409Conflict, new ErrorResponse("REGISTRO_DUPLICADO", "Registro duplicado.")),
        _ => (StatusCodes.Status500InternalServerError, new ErrorResponse("ERRO_INTERNO", "Erro interno do servidor."))
    };

    // ArgumentException acrescenta " (Parameter x)" (traduzido conforme a cultura); removemos o sufixo.
    private static string LimparMensagem(string mensagem)
    {
        var i = mensagem.LastIndexOf(" (", StringComparison.Ordinal);
        return i > 0 ? mensagem[..i] : mensagem;
    }
}
