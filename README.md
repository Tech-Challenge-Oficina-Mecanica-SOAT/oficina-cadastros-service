# oficina-cadastros-service

Cadastros Service da oficina mecânica: dados mestres de **Cliente, Veículo, Peça e Serviço**.
Tech Challenge Fase 4, 15SOAT, PosTech FIAP.

É o serviço do qual os demais dependem: o OS Service consulta clientes e veículos ao abrir uma ordem de serviço,
e o Billing e o Execução consultam peças e serviços. Quando um cliente é desativado, o serviço publica o evento
`cliente.desativado` para os outros reagirem.

## Stack

.NET 8, Clean Architecture, PostgreSQL (EF Core + Npgsql), Amazon SNS (Outbox Pattern), Docker, Kubernetes (EKS), Terraform.

## Estrutura

```
src/
  OficinaCadastros.Domain/           entidades e regras de negócio
  OficinaCadastros.Application/      casos de uso, DTOs, contratos (repositórios, publicador)
  OficinaCadastros.Infrastructure/   EF Core, repositórios, publicadores (SNS e log)
  OficinaCadastros.API/              controllers, middlewares, health checks, Program.cs
tests/
  OficinaCadastros.UnitTests/        regras de domínio e casos de uso (sem banco)
  OficinaCadastros.IntegrationTests/ API completa contra PostgreSQL real (Testcontainers)
  OficinaCadastros.BDD/              cenários em Gherkin (Reqnroll)
k8s/                                 Deployment, Service, HPA
terraform/                           secret da senha, tópico SNS, parâmetros do SSM
scripts/                             deploy, secret, bootstrap do banco, cobertura
wiremock/                            mock do serviço para os outros times
docs/                                OpenAPI, guia de deploy e decisões (adr/)
```

## API

Contrato completo em [`docs/openapi.yaml`](docs/openapi.yaml). Swagger em `/swagger` quando a API está rodando.

| Recurso | Operações |
|---|---|
| Clientes | `POST /clientes`, `GET /clientes/{id}`, `GET /clientes/cpf/{cpf}`, `PUT /clientes/{id}`, `POST /clientes/{id}/desativar` |
| Veículos | `POST /veiculos`, `GET /veiculos/{id}`, `GET /veiculos/placa/{placa}`, `PUT /veiculos/{id}` |
| Peças | `GET /pecas`, `POST /pecas`, `GET /pecas/{id}`, `PUT /pecas/{id}` |
| Serviços | `POST /servicos`, `GET /servicos/{id}`, `PUT /servicos/{id}` |
| Saúde | `GET /health/live`, `GET /health/ready` (este confere o banco) |

Erros seguem o formato `{ "code": "...", "message": "..." }` com 400 (dado inválido), 404 (não encontrado) e 409 (conflito,
como CPF, placa ou código duplicados). A placa e o código da peça são normalizados para maiúsculas.

## Evento publicado

`cliente.desativado` é publicado quando `POST /clientes/{id}/desativar` tem sucesso, com o envelope padrão do projeto
(`eventId`, `eventType`, `eventVersion`, `occurredAt`, `correlationId`, `producer`, `data`). O `data` leva `clienteId`, `motivo`
(`solicitacao_cliente`, `inadimplencia` ou `outro`) e `desativadoEm`. O evento é gravado na tabela `outbox_events` na mesma
transação da desativação e enviado ao SNS por um serviço em segundo plano. Detalhes em
[`docs/adr/0003-outbox-pattern.md`](docs/adr/0003-outbox-pattern.md).

## Rodando localmente

Pré-requisitos: **.NET 8 SDK** e **Docker**.

```bash
docker compose up --build
# Swagger: http://localhost:5001/swagger
# Saúde:   http://localhost:5001/health/ready
```

As migrations são aplicadas na subida (`Database__MigrateOnStartup`). Sem tópico SNS configurado, o evento do outbox é
apenas registrado no log (`[outbox:log]`).

Para os outros times desenvolverem sem depender deste serviço, há um mock com dados fixos:

```bash
docker compose -f docker-compose.wiremock.yml up   # http://localhost:8089, ver wiremock/README.md
```

## Testes

```bash
dotnet test tests/OficinaCadastros.UnitTests           # sem Docker
dotnet test tests/OficinaCadastros.IntegrationTests    # exige Docker
dotnet test tests/OficinaCadastros.BDD                 # exige Docker
bash scripts/coverage.sh                               # tudo + cobertura somada
```

`scripts/coverage.sh` precisa do ReportGenerator (`dotnet tool install --global dotnet-reportgenerator-globaltool`) e falha
se a cobertura de linhas, somando unitários e integração, ficar abaixo de 80% (`COVERAGE_THRESHOLD`). Os testes de
integração e o BDD usam um PostgreSQL em container e rodam com o SDK do .NET 8 (com o runtime 10 forçado por
`DOTNET_ROLL_FORWARD`, o servidor de teste quebra). Estratégia em [`docs/adr/0004-estrategia-de-testes.md`](docs/adr/0004-estrategia-de-testes.md).

## Configuração

Variáveis de ambiente (o `__` equivale a `:` na configuração do .NET):

| Variável | Para quê | Padrão |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | conexão com o PostgreSQL | banco local do compose |
| `Database__MigrateOnStartup` | aplica as migrations ao subir | `false` |
| `Messaging__ClienteDesativadoTopicArn` | ARN do tópico SNS; vazio = só log | vazio |
| `Messaging__Region` | região da AWS do SNS | `us-east-1` |
| `Messaging__PollIntervalSeconds` | intervalo do publicador do outbox | `5` |
| `Messaging__BatchSize` | eventos por ciclo | `20` |
| `Messaging__MaxAttempts` | falhas antes de o evento ser abandonado | `10` |
| `Logging__Console__FormatterName` | `json` (padrão) ou `simple` | `json` |

## Observabilidade

- **Logs em JSON**, uma linha por evento, com o campo `correlationId` nos escopos.
- **Correlation ID:** é o trace id W3C da requisição. Se o chamador mandar o header `traceparent`, a trace continua. Volta
  na resposta em `X-Correlation-Id`.
- **Saúde:** `/health/live` (processo de pé) e `/health/ready` (processo e banco).

## Deploy

CI no GitHub Actions: build, testes, cobertura, build da imagem e `terraform validate`. A imagem é publicada no GHCR ao
mesclar na `main`. O passo a passo para subir um ambiente está em [`docs/deploy.md`](docs/deploy.md) e as decisões em
[`docs/adr/0005-deploy-ghcr-e-clusterip.md`](docs/adr/0005-deploy-ghcr-e-clusterip.md).

## Estado atual e pendências

- **Validado no cluster (homolog):** deploy, migrations, CRUD e a gravação do evento no outbox.
- **Publicação no SNS na nuvem: não validada.** No único teste, o evento ficou pendente com erro `401` de credenciais. A
  hipótese, ainda não confirmada, é o limite de salto do serviço de metadados do nó impedindo o pod de obter credenciais
  (ajuste testado à mão: `http_put_response_hop_limit = 2`). Os testes automatizados usam um publicador falso.
- **Instrumentação do New Relic .NET:** ainda não feita (o Deployment já recebe a licença, mas a imagem não traz o agente).
- **SonarCloud:** ainda não configurado.
- **Decisões a confirmar com o grupo:** registry (GHCR ou ECR), exposição do serviço e prefixo `/cadastros`.
