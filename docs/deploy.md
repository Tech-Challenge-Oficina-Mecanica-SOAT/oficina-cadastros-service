# Deploy do Cadastros Service

Ordem para subir um ambiente (rode no Git Bash, com o lab do AWS Academy ativo e as credenciais renovadas):

1. **Infra base:** `make up ENV=homolog` no `oficina-infra-k8s` (cria VPC, RDS, EKS e o namespace `oficina`).
2. **Recursos AWS do Cadastros:** `cd terraform && terraform init && terraform apply -var env=homolog`
   (secret da senha do banco, parâmetro `api-url` e tópico SNS `cliente.desativado`).
   Para `prod`, use outro workspace: `terraform workspace new prod`.
3. **Banco:** `./scripts/bootstrap-db.sh homolog` (cria a role e o database dentro do cluster).
4. **Imagem:** publicada pelo workflow `deploy.yml` ao mesclar na `main`.
5. **Aplicação:** `IMAGE=ghcr.io/<org>/oficina-cadastros-service:<tag> ./scripts/deploy.sh homolog`

Verificação: `kubectl port-forward svc/oficina-cadastros-service 5001:80 -n oficina` e abrir
`http://localhost:5001/swagger`.

Derrubar: `terraform destroy -var env=homolog` (o database fica no RDS e some junto com ele no `make down`).
