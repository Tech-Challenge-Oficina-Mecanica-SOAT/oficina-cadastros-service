# ADR 0002: Database lógico próprio no RDS compartilhado

**Status:** Aceito · **Data:** 2026-11-03

## Contexto
Cada microsserviço precisa de dados isolados dos outros, mas o laboratório limita custo e a quantidade de recursos
caros (cada instância RDS consome crédito).

## Decisão
O serviço usa o database `oficina_cadastros` e a role `cadastros_service_user` dentro do RDS compartilhado. A senha é
gerada pelo Terraform e guardada no Secrets Manager (`oficina/{env}/cadastros-db-password`). O database e a role são
criados por `scripts/bootstrap-db.sh`, não pelo Terraform, porque o RDS só é alcançável de dentro do cluster (um provider
PostgreSQL rodando no computador do desenvolvedor não conectaria). O script sobe um pod temporário com `psql`, no mesmo
método usado na Fase 3, e é idempotente.

## Consequências
- Isolamento lógico (credenciais e schema separados), sem isolamento de recursos: o RDS continua sendo um ponto único.
- O banco só existe depois do `make up` do `oficina-infra-k8s`.
- O Terraform deste repositório não cria o database; destruí-lo apaga o secret, e o banco some junto com o RDS.
