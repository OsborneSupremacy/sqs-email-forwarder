resource "aws_sns_topic" "silverconcord_inbox" {
  name = "sns-topic-silverconcord-inbox"
}

resource "aws_sns_topic" "osbornesupremancy_inbox" {
  name = "sns-topic-osbornesupremancy-inbox"
}

resource "aws_sns_topic_policy" "silverconcord_inbox_policy" {
  arn = aws_sns_topic.silverconcord_inbox.arn

  policy = jsonencode({
    Version = "2008-10-17"
    Statement = [
      {
        Sid    = "stmt1758404367708"
        Effect = "Allow"
        Principal = {
          Service = "ses.amazonaws.com"
        }
        Action   = "SNS:Publish"
        Resource = aws_sns_topic.silverconcord_inbox.arn
        Condition = {
          StringEquals = {
            "AWS:SourceAccount" = "182571449491"
          }
          StringLike = {
            "AWS:SourceArn" = "arn:aws:ses:*"
          }
        }
      }
    ]
  })
}

resource "aws_sns_topic_policy" "osbornesupremancy_inbox_policy" {
  arn = aws_sns_topic.osbornesupremancy_inbox.arn

  policy = jsonencode({
    Version = "2008-10-17"
    Statement = [
      {
        Sid    = "stmt1758403689843"
        Effect = "Allow"
        Principal = {
          Service = "ses.amazonaws.com"
        }
        Action   = "SNS:Publish"
        Resource = aws_sns_topic.osbornesupremancy_inbox.arn
        Condition = {
          StringEquals = {
            "AWS:SourceAccount" = "182571449491"
          }
          StringLike = {
            "AWS:SourceArn" = "arn:aws:ses:*"
          }
        }
      }
    ]
  })
}
