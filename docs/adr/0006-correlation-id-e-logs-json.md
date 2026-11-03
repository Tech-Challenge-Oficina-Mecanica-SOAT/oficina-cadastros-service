# ADR 0006: Logs em JSON com correlation ID W3C

**Status:** Aceito · **Data:** 2026-11-03

## Contexto
Para seguir um pedido entre serviços, e para o New Relic indexar os campos, os logs precisam ser estruturados e carregar
um identificador comum.

## Decisão
O logger de console usa o formatador JSON do próprio .NET, configurado no `appsettings.json`. Um middleware usa o trace id
W3C da requisição como `correlationId`, põe esse valor como escopo de log e o devolve no header `X-Correlation-Id`. Se o
chamador mandar `traceparent`, a trace continua. O evento do outbox leva o `traceparent` completo no `correlationId`.

## Consequências
- Nenhuma dependência extra de logging.
- Para ler no terminal durante o desenvolvimento, `Logging__Console__FormatterName=simple` volta ao formato legível.
- As consultas SQL do EF Core são registradas como `Warning` ou acima, para o log do outbox (uma consulta a cada 5 s)
  não afogar o restante.
