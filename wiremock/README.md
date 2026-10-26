# Mock do Cadastros Service (Wiremock)

Stub dos 4 endpoints que os outros servicos consomem, baseado em `docs/openapi.yaml` (v1.1.0).

## Subir
```bash
docker compose -f docker-compose.wiremock.yml up
# Base URL: http://localhost:8089
```

## Dados fixos

| Endpoint | Entrada | Resposta |
|---|---|---|
| GET /clientes/cpf/{cpf} | 12345678901 | 200, cliente ativo |
| GET /clientes/cpf/{cpf} | 98765432100 | 200, cliente **inativo** |
| GET /clientes/cpf/{cpf} | outro CPF de 11 digitos | 404 |
| GET /clientes/cpf/{cpf} | formato invalido | 400 |
| GET /veiculos/placa/{placa} | ABC1D23 (qualquer caixa) | 200 |
| GET /veiculos/placa/{placa} | outra placa | 404 |
| GET /pecas/{id} | 33333333-3333-3333-3333-333333333333 | 200, estoque 10 |
| GET /pecas/{id} | 44444444-4444-4444-4444-444444444444 | 200, **estoque 0** |
| GET /pecas/{id} | outro UUID | 404 |
| GET /servicos/{id} | 55555555-5555-5555-5555-555555555555 | 200, Troca de oleo |
| GET /servicos/{id} | 66666666-6666-6666-6666-666666666666 | 200, Alinhamento |
| GET /servicos/{id} | outro UUID | 404 |

O cliente 11111111-1111-1111-1111-111111111111 e dono do veiculo ABC1D23.

## Testar
```bash
curl http://localhost:8089/clientes/cpf/12345678901
curl http://localhost:8089/veiculos/placa/abc1d23
curl http://localhost:8089/pecas/44444444-4444-4444-4444-444444444444
curl -i http://localhost:8089/servicos/00000000-0000-0000-0000-000000000000
```

## Adicionar casos
Crie ou edite um JSON em `wiremock/mappings/`. Mappings mais especificos usam `priority: 1`; o 404 generico usa `5`.
