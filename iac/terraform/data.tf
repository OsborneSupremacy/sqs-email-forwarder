data "aws_caller_identity" "current" {}

data "terraform_remote_state" "lz" {
  backend = "s3"

  config = {
    bucket       = "bro-tfstate"
    key          = "lz"
    region       = "us-east-1"
    use_lockfile = true
  }
}