resource "aws_sqs_queue" "osbornesupremacy_inbox" {
  name                       = "sqs-queue-osbornesupremacy-inbox"
  visibility_timeout_seconds = 300
  max_message_size           = 1048576
}

resource "aws_sqs_queue" "silverconcord_inbox" {
  name                       = "sqs-queue-silverconcord-inbox"
  visibility_timeout_seconds = 300
  max_message_size           = 1048576
}

resource "aws_lambda_event_source_mapping" "sqs_trigger" {
  for_each                           = toset(local.sqs_queue_arns)
  event_source_arn                   = each.value
  function_name                      = aws_lambda_function.forwarder_lambda.arn
  batch_size                         = 10
  maximum_batching_window_in_seconds = 30
  enabled                            = true
}

resource "aws_sqs_queue_policy" "osbornesupremacy_inbox_policy" {
  queue_url = aws_sqs_queue.osbornesupremacy_inbox.url

  policy = jsonencode({
    Version = "2012-10-17"
    Id      = "SQSPolicyForSNS"
    Statement = [
      {
        Sid    = "Allow-SNS-SendMessage"
        Effect = "Allow"
        Principal = {
          Service = "sns.amazonaws.com"
        }
        Action   = "sqs:SendMessage"
        Resource = aws_sqs_queue.osbornesupremacy_inbox.arn
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = aws_sns_topic.osbornesupremancy_inbox.arn
          }
        }
      },
      {
        Sid    = "topic-subscription-arn:aws:sns:us-east-1:182571449491:sns-topic-osbornesupremancy-inbox"
        Effect = "Allow"
        Principal = {
          AWS = "*"
        }
        Action   = "SQS:SendMessage"
        Resource = aws_sqs_queue.osbornesupremacy_inbox.arn
        Condition = {
          ArnLike = {
            "aws:SourceArn" = aws_sns_topic.osbornesupremancy_inbox.arn
          }
        }
      }
    ]
  })
}

resource "aws_sqs_queue_policy" "silverconcord_inbox_policy" {
  queue_url = aws_sqs_queue.silverconcord_inbox.url

  policy = jsonencode({
    Version = "2012-10-17"
    Id      = "SQSPolicyForSNS"
    Statement = [
      {
        Sid    = "Allow-SNS-SendMessage"
        Effect = "Allow"
        Principal = {
          Service = "sns.amazonaws.com"
        }
        Action   = "sqs:SendMessage"
        Resource = aws_sqs_queue.silverconcord_inbox.arn
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = aws_sns_topic.silverconcord_inbox.arn
          }
        }
      },
      {
        Sid    = "topic-subscription-arn:aws:sns:us-east-1:182571449491:sns-topic-silverconcord-inbox"
        Effect = "Allow"
        Principal = {
          AWS = "*"
        }
        Action   = "SQS:SendMessage"
        Resource = aws_sqs_queue.silverconcord_inbox.arn
        Condition = {
          ArnLike = {
            "aws:SourceArn" = aws_sns_topic.silverconcord_inbox.arn
          }
        }
      }
    ]
  })
}
