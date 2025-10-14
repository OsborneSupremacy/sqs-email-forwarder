
resource "aws_lambda_function" "forwarder_lambda" {

  function_name    = "sqs-email-forwarder-lambda"
  description      = "Function that consumes SES notification messages from SQS and forwards emails to configured address"
  handler          = "Sqs.Email.Forwarder::Sqs.Email.Forwarder.Function::FunctionHandler"
  runtime          = "dotnet8"
  architectures    = ["arm64"]
  memory_size      = 256
  timeout          = 30
  filename         = data.archive_file.lambda_function.output_path
  source_code_hash = data.archive_file.lambda_function.output_base64sha256
  role             = aws_iam_role.lambda_role.arn
  environment {
    variables = {
      "MAIL_S3_BUCKETS" = join(",", var.s3_bucket_names)
      "MAIL_SENDERS"    = join(",", var.mail_senders),
      "MAIL_RECIPIENT"  = var.mail_recipient
    }
  }
  tags = local.common_tags
}
