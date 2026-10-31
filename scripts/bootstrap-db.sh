#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "${BASH_SOURCE[0]}")/_ensure-path.sh"

# Cria (de forma idempotente) a role cadastros_service_user e o database oficina_cadastros no RDS.
# O RDS só é alcançável de dentro do cluster, então o psql roda num pod temporário
# (mesmo método do bootstrap-demo-admin.sh da Fase 3).
#
# Pré-requisitos: make up do oficina-infra-k8s concluído e terraform apply deste repo
# (cria o secret oficina/{env}/cadastros-db-password).
#
# Uso: ./scripts/bootstrap-db.sh [homolog|prod]

ENV=${1:-homolog}
POD=psql-cadastros-bootstrap

CLUSTER_NAME=$(aws ssm get-parameter --name "/oficina/$ENV/k8s/cluster-name" --query Parameter.Value --output text)
aws eks update-kubeconfig --region us-east-1 --name "$CLUSTER_NAME" >/dev/null

DB_HOST=$(aws ssm get-parameter --name "/oficina/$ENV/db/endpoint" --query Parameter.Value --output text)
ADMIN_DB=$(aws ssm get-parameter --name "/oficina/$ENV/db/name" --query Parameter.Value --output text)
ADMIN_USER=$(aws ssm get-parameter --name "/oficina/$ENV/db/username" --query Parameter.Value --output text)
ADMIN_PASSWORD=$(aws secretsmanager get-secret-value --secret-id "oficina/$ENV/db-password" --query SecretString --output text)
CADASTROS_PASSWORD=$(aws secretsmanager get-secret-value --secret-id "oficina/$ENV/cadastros-db-password" --query SecretString --output text)

cleanup() { kubectl delete pod "$POD" -n oficina --ignore-not-found >/dev/null 2>&1 || true; }
trap cleanup EXIT

kubectl delete pod "$POD" -n oficina --ignore-not-found >/dev/null
echo "Subindo pod temporario com psql..."
kubectl run "$POD" --image=postgres:16-alpine --restart=Never -n oficina --command -- sleep 120 >/dev/null
kubectl wait --for=condition=Ready "pod/$POD" -n oficina --timeout=90s >/dev/null

echo "Criando role e database (se ainda nao existirem)..."
kubectl exec -i -n oficina "$POD" -- env PGPASSWORD="$ADMIN_PASSWORD" \
  psql -h "$DB_HOST" -U "$ADMIN_USER" -d "$ADMIN_DB" -v ON_ERROR_STOP=1 -v pwd="$CADASTROS_PASSWORD" <<'SQL'
SELECT format('CREATE ROLE cadastros_service_user LOGIN PASSWORD %L', :'pwd')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = 'cadastros_service_user')
\gexec

SELECT format('ALTER ROLE cadastros_service_user PASSWORD %L', :'pwd')
\gexec

GRANT cadastros_service_user TO CURRENT_USER;

SELECT 'CREATE DATABASE oficina_cadastros OWNER cadastros_service_user'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'oficina_cadastros')
\gexec
SQL

echo "Database 'oficina_cadastros' e role 'cadastros_service_user' prontos em $ENV."
