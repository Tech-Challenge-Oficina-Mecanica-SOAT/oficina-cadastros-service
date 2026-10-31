terraform {
  required_version = ">= 1.5"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.5"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# O database e a role são criados por scripts/bootstrap-db.sh (o RDS só é alcançável de dentro
# do cluster). Aqui fica só a senha, guardada no Secrets Manager para o script e o pod lerem.
resource "random_password" "cadastros_db" {
  length  = 24
  special = false
}

resource "aws_secretsmanager_secret" "cadastros_db_password" {
  name = "oficina/${var.env}/cadastros-db-password"

  # Sem janela de recuperação: permite destroy seguido de novo apply com o mesmo nome.
  recovery_window_in_days = 0
}

resource "aws_secretsmanager_secret_version" "cadastros_db_password" {
  secret_id     = aws_secretsmanager_secret.cadastros_db_password.id
  secret_string = random_password.cadastros_db.result
}

# Descoberta do endpoint REST: DNS interno do Service no namespace oficina.
resource "aws_ssm_parameter" "cadastros_api_url" {
  name  = "/oficina/${var.env}/services/cadastros/api-url"
  type  = "String"
  value = "http://oficina-cadastros-service.oficina.svc.cluster.local"
}
