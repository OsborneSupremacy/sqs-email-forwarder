variable "sqs_queue_arns" {
  description = "List of SQS queue ARNs the role should be able to consume from"
  type        = list(string)
}

variable "s3_bucket_names" {
  description = "List of S3 bucket names the role should have read access to"
  type        = list(string)
}

variable "mail_sender_name" {
  description = "The name of the email sender (do not include the email)"
  type = string
}

variable "mail_recipient" {
  description = "value"
  type = string
}