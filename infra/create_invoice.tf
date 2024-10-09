resource "aws_iam_role" "lambda_role_create_invoice" {
  name = "lambda-role-create-invoice"

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

resource "aws_iam_role_policy_attachment" "lambda_basic_execution_create_invoice" {
  role       = aws_iam_role.lambda_role_create_invoice.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

resource "aws_iam_policy" "lambda_s3_policy" {
  name        = "lambda_s3_policy"
  description = "Política IAM para a Lambda acessar o bucket S3 específico"

  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action   = [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject"
      ],
      Effect   = "Allow",
      Resource = "arn:aws:s3:::${var.s3_bucket_name}/*"
    }]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_s3_policy_attachment" {
  role       = aws_iam_role.lambda_role_create_invoice.name
  policy_arn = aws_iam_policy.lambda_s3_policy.arn
}

resource "aws_lambda_function" "lambda_create_invoice" {
  function_name = "lambda-create-invoice"
  role          = aws_iam_role.lambda_role_create_invoice.arn
  handler       = "StarkBank.CreateInvoice::StarkBank.CreateInvoice.Function::FunctionHandler" # Atualize com o handler correto
  runtime       = "dotnet8"

  filename         = "../src/StarkBank/Application/StarkBank.CreateInvoice/bin/Release/net8.0/StarkBank.CreateInvoice.zip"
  source_code_hash = filebase64sha256("../src/StarkBank/Application/StarkBank.CreateInvoice/bin/Release/net8.0/StarkBank.CreateInvoice.zip")
}

resource "aws_cloudwatch_event_rule" "every_3_hours" {
  name                = "every_3_hours"
  description         = "Dispara a função Lambda a cada 3 horas"
  schedule_expression = "rate(3 hours)"
}

resource "aws_lambda_permission" "allow_eventbridge" {
  statement_id  = "AllowExecutionFromEventBridge"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.lambda_create_invoice.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.every_3_hours.arn
}

resource "aws_cloudwatch_event_target" "invoke_lambda" {
  rule      = aws_cloudwatch_event_rule.every_3_hours.name
  target_id = "lambda-create-invoice-target"
  arn       = aws_lambda_function.lambda_create_invoice.arn
}