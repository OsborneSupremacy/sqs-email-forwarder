resource "aws_s3_bucket" "bro-ses-inbox-osbornesupremacy" {
  bucket = "bro-ses-inbox-osbornesupremacy"
}

resource "aws_s3_bucket" "bro-ses-inbox-silverconcord" {
  bucket = "bro-ses-inbox-silverconcord"
}

resource "aws_s3_bucket_policy" "ses_inbox_policy" {
  bucket = "bro-ses-inbox-osbornesupremacy"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowSESPuts"
        Effect = "Allow"
        Principal = {
          Service = "ses.amazonaws.com"
        }
        Action   = "s3:PutObject"
        Resource = "arn:aws:s3:::bro-ses-inbox-osbornesupremacy/*"
        Condition = {
          StringEquals = {
            "aws:Referer" = "182571449491"
          }
        }
      }
    ]
  })
}

resource "aws_s3_bucket_policy" "ses_inbox_silverconcord_policy" {
  bucket = "bro-ses-inbox-silverconcord"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid    = "AllowSESPuts"
        Effect = "Allow"
        Principal = {
          Service = "ses.amazonaws.com"
        }
        Action   = "s3:PutObject"
        Resource = "arn:aws:s3:::bro-ses-inbox-silverconcord/*"
        Condition = {
          StringEquals = {
            "aws:Referer" = "182571449491"
          }
        }
      }
    ]
  })
}

resource "aws_s3_bucket_lifecycle_configuration" "ses_inbox_osbornesupremacy_lifecycle" {
  bucket = "bro-ses-inbox-osbornesupremacy"

  rule {
    id     = "expire-objects-after-7-days"
    status = "Enabled"

    expiration {
      days = 7
    }
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "ses_inbox_silverconcord_lifecycle" {
  bucket = "bro-ses-inbox-silverconcord"

  rule {
    id     = "expire-objects-after-7-days"
    status = "Enabled"

    expiration {
      days = 7
    }
  }
}