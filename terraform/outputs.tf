output "database_name" {
  value = postgresql_database.cadastros.name
}

output "api_url_parameter" {
  value = aws_ssm_parameter.cadastros_api_url.name
}
