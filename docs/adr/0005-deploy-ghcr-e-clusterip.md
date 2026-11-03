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
