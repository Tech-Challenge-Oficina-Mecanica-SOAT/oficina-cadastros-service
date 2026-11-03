# ADR 0001: Clean Architecture em quatro camadas

**Status:** Aceito · **Data:** 2026-11-03

## Contexto
O serviço precisa de regras de negócio testáveis sem banco e de uma API que possa trocar de infraestrutura sem reescrever
casos de uso. A Fase 3 já usava esse padrão e o plano pede para reaproveitá-lo.

## Decisão
Quatro projetos: **Domain** (entidades e validações), **Application** (casos de uso, DTOs e interfaces como
`IClienteRepository` e `IEventPublisher`), **Infrastructure** (EF Core, repositórios, SNS) e **API** (controllers e
middlewares). A dependência aponta sempre para dentro: Application não conhece EF nem AWS.

## Consequências
- Os casos de uso são testados com repositórios falsos em memória, sem Docker.
- Mais arquivos e mais indireção do que um projeto único, o que é aceitável para o tamanho do serviço.
- Erros de domínio saem como `ArgumentException` (viram 400) e os de aplicação como `NotFoundException` e
  `ConflictException` (404 e 409), traduzidos por um único middleware.
