# Tópico SNS do evento cliente.desativado (publisher: Cadastros Service).
# O nome troca "." por "-" porque o SNS não aceita ponto (ver contratos/eventos/schemas.md).
resource "aws_sns_topic" "cliente_desativado" {
  name = "oficina-${var.env}-cliente-desativado"
}

# Descoberta: os consumers (OS Service) leem o ARN daqui para criar a fila SQS e a subscription.
resource "aws_ssm_parameter" "cliente_desativado_topic_arn" {
  name  = "/oficina/${var.env}/messaging/topics/cliente.desativado/arn"
  type  = "String"
  value = aws_sns_topic.cliente_desativado.arn
}

output "cliente_desativado_topic_arn" {
  value = aws_sns_topic.cliente_desativado.arn
}
