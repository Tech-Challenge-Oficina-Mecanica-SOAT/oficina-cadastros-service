#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "${BASH_SOURCE[0]}")/_ensure-path.sh"

# Monta o Secret oficina-cadastros-secrets (mesmo padrão do populate-secret.sh da Fase 3):
# lê Parameter Store + Secrets Manager e grava no namespace oficina.
#
# Uso: ./scripts/populate-secret.sh [homolog|prod]

ENV=${1:-homolog}

echo "Populando Secret K8s do Cadastros para env=$ENV"

DB_ENDPOINT=$(aws ssm get-parameter --name "/oficina/$ENV/db/endpoint" --query Parameter.Value --output text)
DB_PORT=$(aws ssm get-parameter --name "/oficina/$ENV/db/port" --query Parameter.Value --output text)
DB_PASSWORD=$(aws secretsmanager get-secret-value --secret-id "oficina/$ENV/cadastros-db-password" --query SecretString --output text)
TOPIC_ARN=$(aws ssm get-parameter --name "/oficina/$ENV/messaging/topics/cliente.desativado/arn" --query Parameter.Value --output text)

CONNECTION_STRING="Host=$DB_ENDPOINT;Port=$DB_PORT;Database=oficina_cadastros;Username=cadastros_service_user;Password=$DB_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"

kubectl create secret generic oficina-cadastros-secrets \
  --namespace oficina \
  --from-literal=ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
  --from-literal=Messaging__ClienteDesativadoTopicArn="$TOPIC_ARN" \
  --dry-run=client -o yaml | kubectl apply -f -

echo "Secret 'oficina-cadastros-secrets' criado/atualizado no namespace 'oficina'."
