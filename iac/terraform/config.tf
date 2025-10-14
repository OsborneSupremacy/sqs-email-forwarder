provider "aws" {
  region = "us-east-1"

  default_tags {
    tags = {
      Environment = "live"
      Application = "sqs-email-forwarder"
      ManagedBy   = "terraform"
      Owner       = "ses@osbornesupremacy.com"
    }
  }
}

terraform {
  required_providers {
    aws = {
      source = "hashicorp/aws"
    }
  }

  backend "s3" {
    bucket       = "bro-tfstate"
    use_lockfile = true
    key          = "sqs-email-forwarder"
    region       = "us-east-1"
  }
}
