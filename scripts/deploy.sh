#!/usr/bin/env bash
set -euo pipefail

source "$(dirname "${BASH_SOURCE[0]}")/_ensure-path.sh"

# Aplica o Cadastros Service no cluster compartilhado (namespace oficina).
# Uso: IMAGE=ghcr.io/<org>/oficina-cadastros-service:<tag> ./scripts/deploy.sh [homolog|prod]
# Sem IMAGE, usa a tag :latest publicada pelo workflow deploy.yml.

ENV=${1:-homolog}
IMAGE=${IMAGE:-ghcr.io/tech-challenge-oficina-mecanica-soat/oficina-cadastros-service:latest}
RAIZ="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CLUSTER_NAME=$(aws ssm get-parameter --name "/oficina/$ENV/k8s/cluster-name" --query Parameter.Value --output text)
aws eks update-kubeconfig --region us-east-1 --name "$CLUSTER_NAME" >/dev/null

if ! kubectl get namespace oficina >/dev/null 2>&1; then
  echo "Namespace 'oficina' nao existe. Rode 'make up' no oficina-infra-k8s primeiro."
  exit 1
fi

"$RAIZ/scripts/populate-secret.sh" "$ENV"

echo "Aplicando manifestos (imagem: $IMAGE)"
sed "s|__IMAGE__|$IMAGE|g" "$RAIZ/k8s/deployment.yaml" | kubectl apply -f -
kubectl apply -f "$RAIZ/k8s/service.yaml"
kubectl apply -f "$RAIZ/k8s/hpa.yaml"

kubectl rollout status deployment/oficina-cadastros-service -n oficina --timeout=5m

echo "Deploy concluido. URL interna: http://oficina-cadastros-service.oficina.svc.cluster.local"
echo "Teste rapido:  kubectl port-forward svc/oficina-cadastros-service 5001:80 -n oficina"
echo "               curl http://localhost:5001/health/ready"
