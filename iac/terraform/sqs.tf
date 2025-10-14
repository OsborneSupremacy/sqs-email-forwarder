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
