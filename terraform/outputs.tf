output "db_password_secret_name" {
  value = aws_secretsmanager_secret.cadastros_db_password.name
}

output "api_url_parameter" {
  value = aws_ssm_parameter.cadastros_api_url.name
}
