locals {
  common_tags = {
    Environment = "live"
    Application = "sqs-email-forwarder"
    ManagedBy   = "terraform"
    Owner       = "ses@osbornesupremacy.com"
  }

  project_directory = "../../src/Sqs.Email.Forwarder"

  build_command     = <<EOT
      cd ${local.project_directory}
      dotnet publish -o bin/publish -c Release --framework "net8.0" /p:GenerateRuntimeConfigurationFiles=true --runtime linux-arm64 --self-contained false
    EOT

  build_output_path = "${local.project_directory}/bin/publish"
  publish_zip_path  = "${local.project_directory}/bin/lambda_function.zip"
}