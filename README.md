# oficina-cadastros-service

Cadastros Service (master data): Cliente, Veículo, Peça e Serviço.
Tech Challenge Fase 4 - 15SOAT PosTech FIAP.

## Stack
.NET 8, Clean Architecture, PostgreSQL, EF Core, SNS (Outbox, semana 3).

## Primeira vez
```bash
./bootstrap.sh          # cria OficinaCadastros.sln e liga os projetos
dotnet build && dotnet test
```

## Rodando local
```bash
docker compose up --build
# Swagger: http://localhost:5001/swagger
# Health:  http://localhost:5001/health/ready
```

## Estrutura
`src/` Domain, Application, Infrastructure, API · `tests/` unitários · `terraform/` database + SSM · `docs/` OpenAPI e ADRs.

## Contrato
OpenAPI em `docs/openapi.yaml` (fonte: `contratos/openapi/cadastros.yaml` do pacote da Fase 4).
