# SQS Email Forwarder

This project consists of the Terraform to provision the infrastucture to facilitate an SQS Email Forwarder, as well as a Lambda function to process the emails.

This is tailored to my personal use case, but the pattern can be adapted to suit others.

The architecture is as follows:

## Not Part of this Repo

* SES Rule Set.
    * Delivers email to S3.
    * Publishes to SNS Topic.

## Part of this Repo

* S3 bucket to store emails, with lifecycle policy to delete after 7 days (one per email domain).
* SNS topics (one per email domain).
* SNS topic subscriptions to SQS queues.
* SQS queues (one per email domain).
* SQL triggers to invoke Lambda function (one per email domain).
* Lambda function to process SQS messages and forward emails.
* All necessary resource policies and IAM roles/policies.
