# ADR 0005: Imagem no GHCR, Service ClusterIP e deploy por script

**Status:** Proposto, a confirmar com o grupo · **Data:** 2026-11-03

## Contexto
O plano da Fase 4 cita ECR e `repository_dispatch`, mas a Fase 3 já usa GHCR com `ghcr-pull-secret` no cluster. As
credenciais do AWS Academy expiram em poucas horas, então não dá para guardá-las como secrets do GitHub. Os serviços
rodam no mesmo cluster e no namespace `oficina`.

## Decisão
- A imagem é publicada no **GHCR** pelo workflow `deploy.yml`, usando o `GITHUB_TOKEN` (sem credenciais da AWS).
- O Service é **ClusterIP**: os outros serviços chamam por DNS interno. O `api-url` publicado no SSM é
  `http://oficina-cadastros-service.oficina.svc.cluster.local`.
- O deploy no cluster é feito por `scripts/deploy.sh`, rodado por uma pessoa com a sessão do lab ativa.
- A API serve na raiz, sem o prefixo `/cadastros` que aparece no servidor do OpenAPI.

## Consequências
- Se o grupo escolher ECR, mudam a linha da imagem, o `imagePullSecrets` e o workflow.
- Se houver um gateway com o prefixo `/cadastros` na frente, é preciso `UsePathBase("/cadastros")`.
- Não há deploy automático a cada merge, é uma ação manual.

## Atualização: validação do SNS na nuvem (homolog)

A publicação de `cliente.desativado` no SNS foi validada no cluster (log `publicado no SNS`, com MessageId). Foram necessários dois ajustes:

- **Limite de salto do IMDS em 2**, via launch template no node group (infra-k8s). Com o padrão (1), os pods não alcançavam as credenciais do nó e a chamada falhava com 401.
- **Policy de recurso no tópico SNS** (`terraform/sns-policy.tf`), liberando `SNS:Publish` para a `LabEksNodeRole`. O Learner Lab bloqueia `iam:PutRolePolicy`, então a permissão não pode ir na role.

Consequência de segurança: com o salto em 2, qualquer pod do cluster compartilhado alcança as credenciais da role do nó. É aceitável no Learner Lab. Em produção, o caminho correto é uma role por serviço (IRSA).

## Validacao do SNS na nuvem
Registrada em 2026-11-09 (homolog).
