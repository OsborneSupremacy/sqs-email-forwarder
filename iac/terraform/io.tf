variable "role_name" {
  description = "Name of the IAM role to create for the Lambda function"
  type        = string
  default     = "sqs-email-forwarder-lambda-role"
}

variable "sqs_queue_arns" {
  description = "List of SQS queue ARNs the role should be able to consume from"
  type        = list(string)
  default     = []
}

variable "s3_bucket_names" {
  description = "List of S3 bucket names the role should have read access to"
  type        = list(string)
  default     = []
}
