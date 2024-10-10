resource "aws_sns_topic" "sns_invoice_payment" {
  name = var.sns_topic_name
}

resource "aws_sqs_queue" "sqs_invoice_payment" {
  name                       = var.sqs_queue_name
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.dlq_invoice_payment.arn
    maxReceiveCount     = 3
  })
}

resource "aws_sqs_queue" "dlq_invoice_payment" {
  name = "${var.sqs_queue_name}-dlq"
}

resource "aws_sqs_queue_policy" "sqs_queue_invoice_payment_policy" {
  queue_url = aws_sqs_queue.sqs_invoice_payment.id

  policy = jsonencode({
    Version = "2012-10-17",
    Id      = "sqs-policy",
    Statement = [
      {
        Effect    = "Allow",
        Principal = "*",
        Action    = "SQS:SendMessage",
        Resource  = aws_sqs_queue.sqs_invoice_payment.arn,
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = aws_sns_topic.sns_invoice_payment.arn
          }
        }
      }
    ]
  })
}

resource "aws_sns_topic_subscription" "sns_to_sqs_subscription" {
  topic_arn = aws_sns_topic.sns_invoice_payment.arn
  protocol  = "sqs"
  endpoint  = aws_sqs_queue.sqs_invoice_payment.arn
  depends_on = [aws_sqs_queue_policy.sqs_queue_invoice_payment_policy]
}
