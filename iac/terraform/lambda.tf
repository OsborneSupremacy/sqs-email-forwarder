locals {
  publish_zip_path = "${path.module}/../../src/Sqs.Email.Forwarder/src/Sqs.Email.Forwarder/bin/Sqs.Email.Forwarder.zip"
}

resource "aws_lambda_function" "forwarder_lambda" {
  function_name    = "sqs-email-forwarder-lambda"
  description      = "Function that consumes SES notification messages from SQS and forwards emails to configured address"
  handler          = "Sqs.Email.Forwarder::Sqs.Email.Forwarder.Function::FunctionHandler"
  runtime          = "dotnet10"
  architectures    = ["arm64"]
  memory_size      = 256
  timeout          = 300
  filename         = local.publish_zip_path
  source_code_hash = filebase64sha256(local.publish_zip_path)
  role             = aws_iam_role.lambda_role.arn
  environment {
    variables = {
      "MAIL_S3_BUCKETS" = join(",", local.s3_bucket_names)
      "MAIL_SENDERS"    = join(",", local.mail_senders),
      "MAIL_RECIPIENT"  = local.mail_recipient,
      "STAGING_BUCKET"  = local.staging_bucket_name
    }
  }
}
