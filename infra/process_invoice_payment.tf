resource "aws_lambda_function" "lambda_process_invoice_payment" {
  function_name = "lambda-process-invoice-payment"
  role          = aws_iam_role.lambda_role_process_invoice_payment.arn
  handler       = "StarkBank.ProcessInvoicePayment::StarkBank.ProcessInvoicePayment.Function::FunctionHandler"
  runtime       = "dotnet8"
  filename         = "./lambda_function_payload.zip"
  source_code_hash = filebase64sha256("./lambda_function_payload.zip")
  environment {
    variables = {
      SQS_QUEUE_URL   = aws_sqs_queue.sqs_invoice_payment.url,
      S3_BUCKET_NAME  = var.s3_bucket_name
    }
  }
}

resource "aws_lambda_event_source_mapping" "sqs_to_lambda" {
  event_source_arn = aws_sqs_queue.sqs_invoice_payment.arn
  function_name    = aws_lambda_function.lambda_process_invoice_payment.arn
  batch_size       = 10 
  enabled          = true
}

resource "aws_iam_role" "lambda_role_process_invoice_payment" {
  name = "lambda-role-process-invoice-payment"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action    = "sts:AssumeRole",
      Effect    = "Allow",
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy" "lambda_sqs_policy" {
  name = "lambda_sqs_policy"
  role = aws_iam_role.lambda_role_process_invoice_payment.id

  policy = jsonencode({
    Version : "2012-10-17",
    Statement : [
      {
        Effect : "Allow",
        Action : [
          "sqs:ReceiveMessage",
          "sqs:DeleteMessage",
          "sqs:GetQueueAttributes"
        ],
        Resource : aws_sqs_queue.sqs_invoice_payment.arn
      }
    ]
  })
}

resource "aws_iam_role_policy" "lambda_s3_policy" {
  name = "lambda_s3_policy"
  role = aws_iam_role.lambda_role_process_invoice_payment.id

  policy = jsonencode({
    Version : "2012-10-17",
    Statement : [
      {
        Effect : "Allow",
        Action : [
          "s3:PutObject",
          "s3:GetObject",
          "s3:DeleteObject",
          "s3:ListBucket"
        ],
        Resource : [
          "arn:aws:s3:::${var.s3_bucket_name}/*",
          "arn:aws:s3:::${var.s3_bucket_name}"
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_role_process_invoice_payment.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}
