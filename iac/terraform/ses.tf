resource "aws_ses_receipt_rule" "osbornesupremacy_inbox" {
  name          = "ses-rule-osbornesupremacy-inbox-to-s3"
  rule_set_name = data.terraform_remote_state.lz.outputs.ses_inbox_ruleset_main.name
  enabled       = true
  scan_enabled  = false
  tls_policy    = "Optional"
  recipients    = ["osbornesupremacy.com"]

  s3_action {
    position     = 1
    bucket_name  = aws_s3_bucket.bro-ses-inbox-osbornesupremacy.bucket
    iam_role_arn = "arn:aws:iam::${local.account_id}:role/ses-inbox-role"
  }

  sns_action {
    position  = 2
    topic_arn = aws_sns_topic.osbornesupremancy_inbox.arn
    encoding  = "Base64"
  }
}

resource "aws_ses_receipt_rule" "silverconcord_inbox_to_s3" {
  name          = "ses-rule-silverconcord-inbox-to-s3"
  rule_set_name = data.terraform_remote_state.lz.outputs.ses_inbox_ruleset_main.name
  enabled       = true
  scan_enabled  = false
  tls_policy    = "Optional"
  recipients    = ["silverconcord.com"]

  s3_action {
    position     = 1
    bucket_name  = aws_s3_bucket.bro-ses-inbox-silverconcord.bucket
    iam_role_arn = "arn:aws:iam::${local.account_id}:role/ses-inbox-role"
  }

  sns_action {
    position  = 2
    topic_arn = aws_sns_topic.silverconcord_inbox.arn
    encoding  = "Base64"
  }
}
