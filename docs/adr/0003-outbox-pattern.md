# ADR 0003: Outbox Pattern para publicar `cliente.desativado`

**Status:** Aceito · **Data:** 2026-11-03

## Contexto
Desativar um cliente muda o banco e precisa avisar os outros serviços. Gravar e publicar em sistemas diferentes sem
transação comum pode deixar o banco alterado sem evento (ou o contrário) se o processo cair no meio.

## Decisão
O evento é gravado na tabela `outbox_events` no mesmo `SaveChanges` que desativa o cliente. Um `BackgroundService`
(`OutboxPublisherService`) consulta os pendentes a cada 5 segundos, publica no SNS e grava `PublishedAt`. Falhas
incrementam `Attempts` e guardam `LastError`; depois de 10 falhas o evento deixa de ser tentado e fica na tabela para análise.
Sem ARN de tópico configurado, um publicador de log substitui o SNS (desenvolvimento local).

## Consequências
- Entrega **pelo menos uma vez**: se o processo cair entre publicar e gravar `PublishedAt`, o evento é reenviado, e com
  mais de uma réplica pode haver duplicata. O contrato de eventos já exige consumers idempotentes por `eventId`.
- Atraso de alguns segundos entre a desativação e a publicação.
- Eventos abandonados exigem intervenção manual.
- O `correlationId` do evento é o `traceparent` W3C da requisição que o originou.
