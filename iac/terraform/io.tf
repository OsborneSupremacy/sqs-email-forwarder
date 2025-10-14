variable "s3_bucket_names" {
  description = "List of S3 bucket names the role should have read access to"
  type        = list(string)
}

variable "mail_senders" {
  description = "The sender email addresses"
  type        = list(string)
}

variable "mail_recipient" {
  description = "value"
  type        = string
}
