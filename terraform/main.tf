terraform {
  required_version = ">= 1.5"
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    postgresql = {
      source  = "cyrilgdn/postgresql"
      version = "~> 1.22"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

provider "postgresql" {
  host      = var.db_host
  port      = var.db_port
  username  = var.db_admin_username
  password  = var.db_admin_password
  sslmode   = "require"
  superuser = false
}

# Database lógico no RDS compartilhado
resource "postgresql_role" "cadastros" {
  name     = "cadastros_service_user"
  login    = true
  password = var.cadastros_db_password
}

resource "postgresql_database" "cadastros" {
  name  = "oficina_cadastros"
  owner = postgresql_role.cadastros.name
}

# Grants apenas nesse database
resource "postgresql_grant" "cadastros_connect" {
  database    = postgresql_database.cadastros.name
  role        = postgresql_role.cadastros.name
  object_type = "database"
  privileges  = ["CONNECT", "CREATE"]
}

# Descoberta do endpoint (URL real entra na semana 5, após o deploy no EKS)
resource "aws_ssm_parameter" "cadastros_api_url" {
  name  = "/oficina/${var.env}/services/cadastros/api-url"
  type  = "String"
  value = "pendente-deploy"

  lifecycle {
    ignore_changes = [value]
  }
}
