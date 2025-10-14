locals {

  project_directory = "../../src/Sqs.Email.Forwarder"

  mail_senders   = ["ses@osbornesupremacy.com", "ses@silverconcord.com"]
  mail_recipient = "osborne.ben@gmail.com"

  build_command = <<EOT
      cd ${local.project_directory}
      dotnet publish -o bin/publish -c Release --framework "net8.0" /p:GenerateRuntimeConfigurationFiles=true --runtime linux-arm64 --self-contained false
    EOT

  build_output_path = "${local.project_directory}/bin/publish"
  publish_zip_path  = "${local.project_directory}/bin/lambda_function.zip"

  sqs_queue_arns = [
    aws_sqs_queue.silverconcord_inbox.arn,
    aws_sqs_queue.osbornesupremacy_inbox.arn
  ]

  s3_bucket_names = [
    aws_s3_bucket.bro-ses-inbox-osbornesupremacy.bucket,
    aws_s3_bucket.bro-ses-inbox-silverconcord.bucket
  ]
}
