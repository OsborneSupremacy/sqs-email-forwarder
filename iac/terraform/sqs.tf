resource "aws_sqs_queue" "osbornesupremacy_inbox" {
  name             = "sqs-queue-osbornesupremacy-inbox"
  max_message_size = 1048576
}

resource "aws_sqs_queue" "silverconcord_inbox" {
  name             = "sqs-queue-silverconcord-inbox"
  max_message_size = 1048576
}
