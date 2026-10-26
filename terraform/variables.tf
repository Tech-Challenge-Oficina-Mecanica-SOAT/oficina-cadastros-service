variable "env" {
  type        = string
  description = "homolog ou prod"
}

variable "aws_region" {
  type    = string
  default = "us-east-1"
}

variable "db_host" {
  type        = string
  description = "Endpoint do RDS compartilhado (output do oficina-infra-db)"
}

variable "db_port" {
  type    = number
  default = 5432
}

variable "db_admin_username" {
  type    = string
  default = "postgres"
}

variable "db_admin_password" {
  type      = string
  sensitive = true
}

variable "cadastros_db_password" {
  type      = string
  sensitive = true
}
