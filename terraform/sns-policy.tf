# Permite que os pods do cluster (via role do no, LabEksNodeRole) publiquem no topico.
# No Learner Lab nao e possivel anexar policy a role, entao a permissao vai no recurso.
data "aws_iam_roles" "eks_node" {
  name_regex = "LabEksNodeRole"
}

data "aws_caller_identity" "sns_policy" {}

resource "aws_sns_topic_policy" "cliente_desativado" {
  arn = aws_sns_topic.cliente_desativado.arn

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "DonoDaContaGerencia"
        Effect    = "Allow"
        Principal = { AWS = "*" }
        Action    = ["SNS:GetTopicAttributes", "SNS:SetTopicAttributes", "SNS:AddPermission", "SNS:RemovePermission", "SNS:DeleteTopic", "SNS:Subscribe", "SNS:ListSubscriptionsByTopic", "SNS:Publish"]
        Resource  = aws_sns_topic.cliente_desativado.arn
        Condition = { StringEquals = { "AWS:SourceOwner" = data.aws_caller_identity.sns_policy.account_id } }
      },
      {
        Sid       = "PodsDoClusterPublicam"
        Effect    = "Allow"
        Principal = { AWS = tolist(data.aws_iam_roles.eks_node.arns)[0] }
        Action    = "SNS:Publish"
        Resource  = aws_sns_topic.cliente_desativado.arn
      }
    ]
  })
}
