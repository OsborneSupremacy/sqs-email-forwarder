output "lambda_role_arn" {
  description = "ARN of the IAM role created for Lambda"
  value       = aws_iam_role.lambda_role.arn
}

output "lambda_role_name" {
  description = "Name of the IAM role created for Lambda"
  value       = aws_iam_role.lambda_role.name
}
