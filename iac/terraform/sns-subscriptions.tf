resource "aws_sns_topic_subscription" "osbornesupremancy_to_sqs" {
  topic_arn = aws_sns_topic.osbornesupremancy_inbox.arn
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.osbornesupremacy_inbox.arn
}

resource "aws_sns_topic_subscription" "silverconcord_to_sqs" {
  topic_arn = aws_sns_topic.silverconcord_inbox.arn
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.silverconcord_inbox.arn
}
