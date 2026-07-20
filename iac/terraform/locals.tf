locals {

  account_id = data.aws_caller_identity.current.account_id

  mail_senders   = ["ses@osbornesupremacy.com", "ses@silverconcord.com"]
  mail_recipient = "osborne.ben@gmail.com"

  sqs_queue_arns = [
    aws_sqs_queue.silverconcord_inbox.arn,
    aws_sqs_queue.osbornesupremacy_inbox.arn
  ]

  s3_bucket_names = [
    aws_s3_bucket.bro-ses-inbox-osbornesupremacy.bucket,
    aws_s3_bucket.bro-ses-inbox-silverconcord.bucket
  ]

  staging_bucket_name = aws_s3_bucket.staging_bucket.bucket
}

